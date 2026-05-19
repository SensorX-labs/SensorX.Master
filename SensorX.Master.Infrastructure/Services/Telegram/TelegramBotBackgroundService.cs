using System.Collections.Concurrent;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SensorX.Master.Infrastructure.Services.Telegram;

// ─── Worker ───────────────────────────────────────────────────────────────────

public partial class TelegramBotBackgroundService : BackgroundService
{
    private const string BotPollingMutexName = "SensorX.Master.TelegramBotPolling";
    private static readonly ConcurrentDictionary<int, byte> ProcessedUpdates = new();

    private readonly ILogger<TelegramBotBackgroundService> _logger;
    private readonly ITelegramBotClient _botClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TelegramOptions _options;
    private CancellationTokenSource? _pollingCancellationTokenSource;

    public TelegramBotBackgroundService(
        ILogger<TelegramBotBackgroundService> logger,
        ITelegramBotClient botClient,
        IServiceProvider serviceProvider,
        IHttpClientFactory httpClientFactory,
        IOptions<TelegramOptions> options)
    {
        _logger = logger;
        _botClient = botClient;
        _serviceProvider = serviceProvider;
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    // khởi động worker
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var pollingMutex = new Mutex(false, BotPollingMutexName);
        using var pollingCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        _pollingCancellationTokenSource = pollingCancellationTokenSource;
        var ownsPolling = false;

        try
        {
            ownsPolling = pollingMutex.WaitOne(0);
            if (!ownsPolling)
            {
                _logger.LogWarning("[TelegramBot] PID {ProcessId}: Đã có một instance khác đang polling. Bỏ qua Telegram bot ở process này.", Environment.ProcessId);
                return;
            }

            var me = await _botClient.GetMe(stoppingToken);
            _logger.LogInformation("[TelegramBot] PID {ProcessId}: Bot @{BotName} đã khởi động và đang lắng nghe...", Environment.ProcessId, me.Username);

            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                errorHandler: HandlePollingErrorAsync,
                receiverOptions: new ReceiverOptions { AllowedUpdates = [] },
                cancellationToken: pollingCancellationTokenSource.Token
            );

            await Task.Delay(Timeout.Infinite, pollingCancellationTokenSource.Token);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested || pollingCancellationTokenSource.IsCancellationRequested)
        {
            _logger.LogInformation("[TelegramBot] PID {ProcessId}: Bot đã dừng.", Environment.ProcessId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TelegramBot] Lỗi khởi động Bot");
        }
        finally
        {
            if (ownsPolling)
            {
                pollingMutex.ReleaseMutex();
            }

            _pollingCancellationTokenSource = null;
        }
    }

    // xử lý tin nhắn
    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        if (!ProcessedUpdates.TryAdd(update.Id, 0))
        {
            _logger.LogWarning("[TelegramBot] PID {ProcessId}: Bỏ qua update trùng: {UpdateId}", Environment.ProcessId, update.Id);
            return;
        }

        if (update.Message is not { Text: { } messageText } message) return;
        var chatId = message.Chat.Id;

        // check admin
        if (string.IsNullOrEmpty(_options.AdminId) || _options.AdminId == "0")
        {
            if (messageText.Trim() == _options.SetupSecret)
            {
                _options.AdminId = chatId.ToString();
                SaveAdminId(chatId.ToString());
                await botClient.SendMessage(chatId, "Đã xác nhận Admin! Bot đang chạy trực tiếp trong Master Service.", cancellationToken: ct);
            }
            else
            {
                await botClient.SendMessage(chatId, "Bot chưa được ghép đôi. Hãy nhập mật mã thiết lập.", cancellationToken: ct);
            }
            return;
        }

        // nếu là người lạ thì chặn
        if (chatId.ToString() != _options.AdminId)
        {
            _logger.LogWarning("[TelegramBot] Truy cập trái phép từ ChatId: {ChatId}", chatId);
            return;
        }

        var statusMsg = await botClient.SendMessage(chatId, "🔍 Đang phân tích...", cancellationToken: ct);

        try
        {
            // Gọi 9router phân tích intent
            var skillName = await ResolveSkillAsync(messageText, ct);
            string responseText;

            if (skillName is not null)
            {
                // Dispatch MediatR
                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                responseText = skillName switch
                {
                    "QUOTE_PENDING"                           => await HandleQuotesAsync(mediator, "Pending", ct),
                    "QUOTE_APPROVED"                          => await HandleQuotesAsync(mediator, "Approved", ct),
                    var s when s.StartsWith("QUOTE_APPROVE:") => await HandleQuoteApprove(mediator, s["QUOTE_APPROVE:".Length..].Trim(), ct),
                    _                                         => "⚠️ Skill chưa được implement."
                };

                _logger.LogInformation("[TelegramBot] Đã xử lý skill: {Skill}", skillName);
            }
            else
            {
                responseText = "🤷 Tôi không hiểu yêu cầu này.\n\nTôi có thể giúp bạn:\n" +
                               "• _Báo giá chờ duyệt_\n" +
                               "• _Danh sách báo giá đã duyệt_\n" +
                               "• _Duyệt báo giá (ví dụ: Duyệt báo giá QT-2024-001)_";
            }

            await botClient.DeleteMessage(chatId, statusMsg.MessageId, ct);
            await SendLongMessageAsync(botClient, chatId, responseText, ct);
        }
        catch (Exception ex)
        {
            await botClient.EditMessageText(chatId, statusMsg.MessageId,
                $"❌ Lỗi xử lý:\n`{ex.Message}`", parseMode: ParseMode.Markdown, cancellationToken: ct);
            _logger.LogError(ex, "[TelegramBot] Lỗi xử lý tin nhắn");
        }
    }

    private static async Task SendLongMessageAsync(ITelegramBotClient bot, long chatId, string text, CancellationToken ct)
    {
        const int max = 4000;
        if (string.IsNullOrWhiteSpace(text)) return;
        for (int i = 0; i < text.Length; i += max)
        {
            var chunk = text.Substring(i, Math.Min(max, text.Length - i));
            try { await bot.SendMessage(chatId, chunk, parseMode: ParseMode.Markdown, cancellationToken: ct); }
            catch { await bot.SendMessage(chatId, chunk, cancellationToken: ct); }
        }
    }

    private void SaveAdminId(string adminId)
    {
        try
        {
            const string path = "appsettings.json";
            if (!File.Exists(path)) return;
            var json = File.ReadAllText(path);
            var node = System.Text.Json.Nodes.JsonNode.Parse(json);
            if (node?["Telegram"] != null)
            {
                node["Telegram"]!["AdminId"] = adminId;
                File.WriteAllText(path, node.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        catch { /* Bỏ qua nếu chạy trong Docker */ }
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken ct)
    {
        if (ex.Message.Contains("409: Conflict", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("[TelegramBot] PID {ProcessId}: Một instance khác đang dùng cùng bot token. Dừng polling Telegram ở process này.", Environment.ProcessId);
            _pollingCancellationTokenSource?.Cancel();
            return Task.CompletedTask;
        }

        _logger.LogError(ex, "[TelegramBot] Lỗi polling Telegram API");
        return Task.CompletedTask;
    }
}
