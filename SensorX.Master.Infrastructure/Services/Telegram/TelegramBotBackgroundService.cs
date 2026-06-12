using System.Collections.Concurrent;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SensorX.Master.Infrastructure.Services.Telegram;

public partial class TelegramBotBackgroundService : BackgroundService
{
    private static readonly ConcurrentDictionary<int, byte> ProcessedUpdates = new();
    private static int _pollingStarted;
    // Giới hạn tối đa số update ID được lưu để tránh memory leak
    private const int MaxProcessedUpdates = 500;

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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var pollingCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        _pollingCancellationTokenSource = pollingCancellationTokenSource;
        var ownsPolling = false;

        try
        {
            ownsPolling = Interlocked.CompareExchange(ref _pollingStarted, 1, 0) == 0;
            if (!ownsPolling)
            {
                _logger.LogWarning("[TelegramBot] PID {ProcessId}: Another instance is already polling.", Environment.ProcessId);
                return;
            }

            User me;
            try
            {
                me = await _botClient.GetMe(stoppingToken);
            }
            catch (ApiRequestException apiEx) when (IsFatalApiError(apiEx))
            {
                _logger.LogCritical(
                    "[TelegramBot] PID {ProcessId}: Fatal API error during startup (Code={ErrorCode}). "
                    + "Bot token may be revoked or invalid. Polling will NOT start. Fix the token and restart the service.",
                    Environment.ProcessId, apiEx.ErrorCode);
                return;
            }

            _logger.LogInformation("[TelegramBot] PID {ProcessId}: Bot @{BotName} started.", Environment.ProcessId, me.Username);

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
            _logger.LogInformation("[TelegramBot] PID {ProcessId}: Bot stopped.", Environment.ProcessId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TelegramBot] Startup error");
        }
        finally
        {
            if (ownsPolling)
            {
                Interlocked.Exchange(ref _pollingStarted, 0);
            }

            _pollingCancellationTokenSource = null;
        }
    }

    /// <summary>
    /// Lỗi fatal từ Telegram API: token bị revoke (401), bot bị block/banned (403).
    /// Những lỗi này không thể tự phục hồi bằng cách retry.
    /// </summary>
    private static bool IsFatalApiError(ApiRequestException ex)
        => ex.ErrorCode is 401 or 403;

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        if (!ProcessedUpdates.TryAdd(update.Id, 0))
        {
            _logger.LogWarning("[TelegramBot] PID {ProcessId}: Skip duplicate update: {UpdateId}", Environment.ProcessId, update.Id);
            return;
        }

        // Giới hạn kích thước dictionary để tránh memory leak khi chạy dài ngày
        if (ProcessedUpdates.Count > MaxProcessedUpdates)
        {
            foreach (var key in ProcessedUpdates.Keys.OrderBy(k => k).Take(ProcessedUpdates.Count - MaxProcessedUpdates))
                ProcessedUpdates.TryRemove(key, out _);
        }

        if (update.Message is not { Text: { } messageText } message) return;
        var chatId = message.Chat.Id;
        var normalizedText = messageText.Trim();

        if (string.IsNullOrEmpty(_options.AdminId) || _options.AdminId == "0")
        {
            if (normalizedText == _options.SetupSecret)
            {
                _options.AdminId = chatId.ToString();
                SaveAdminId(chatId.ToString());
                await botClient.SendMessage(chatId, "Đã xác nhận Admin. Bot đang chạy trực tiếp trong Master Service.", cancellationToken: ct);
            }
            else
            {
                await botClient.SendMessage(chatId, "Bot chưa được ghép đôi. Hãy nhập mật mã thiết lập.", cancellationToken: ct);
            }
            return;
        }

        if (chatId.ToString() != _options.AdminId)
        {
            _logger.LogWarning("[TelegramBot] Unauthorized access from ChatId: {ChatId}", chatId);
            return;
        }

        if (normalizedText.Equals("/help", StringComparison.OrdinalIgnoreCase) ||
            normalizedText.Equals("/menu", StringComparison.OrdinalIgnoreCase) ||
            normalizedText.Equals("help", StringComparison.OrdinalIgnoreCase) ||
            normalizedText.Equals("menu", StringComparison.OrdinalIgnoreCase))
        {
            await SendLongMessageAsync(botClient, chatId, FormatHelpMessage(), ct);
            return;
        }

        var statusMsg = await botClient.SendMessage(chatId, "🔍 Đang phân tích...", cancellationToken: ct);

        try
        {
            var skillName = await ResolveSkillAsync(normalizedText, ct);
            string responseText;

            if (skillName is not null)
            {
                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                responseText = skillName switch
                {
                    "HELP"                                     => FormatHelpMessage(),
                    "QUOTE_PENDING"                            => await HandleQuotesAsync(mediator, "Pending", ct),
                    "QUOTE_APPROVED"                           => await HandleQuotesAsync(mediator, "Approved", ct),
                    var s when s.StartsWith("QUOTE_DETAIL:")   => await HandleQuoteDetailAsync(mediator, s["QUOTE_DETAIL:".Length..].Trim(), ct),
                    var s when s.StartsWith("QUOTE_APPROVE:")  => await HandleQuoteApprove(mediator, s["QUOTE_APPROVE:".Length..].Trim(), ct),
                    var s when s.StartsWith("QUOTE_REJECT:")   => await HandleQuoteRejectAsync(mediator, s["QUOTE_REJECT:".Length..].Trim(), ct),
                    _                                          => "⚠️ Skill chưa được implement."
                };

                _logger.LogInformation("[TelegramBot] Processed skill: {Skill}", skillName);
            }
            else
            {
                responseText = "🤷 Tôi chưa hiểu yêu cầu này.\n\n" +
                               "Bạn có thể thử:\n" +
                               "• `Báo giá chờ duyệt`\n" +
                               "• `Chi tiết báo giá QT-2024-001`\n" +
                               "• `Duyệt báo giá QT-2024-001`\n" +
                               "• `/help`";
            }

            await botClient.DeleteMessage(chatId, statusMsg.MessageId, ct);
            await SendLongMessageAsync(botClient, chatId, responseText, ct);
        }
        catch (Exception ex)
        {
            await botClient.EditMessageText(
                chatId,
                statusMsg.MessageId,
                $"❌ Lỗi xử lý:\n`{ex.Message}`",
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);
            _logger.LogError(ex, "[TelegramBot] Message handling error");
        }
    }

    private static async Task SendLongMessageAsync(ITelegramBotClient bot, long chatId, string text, CancellationToken ct)
    {
        const int max = 4000;
        if (string.IsNullOrWhiteSpace(text)) return;

        for (int i = 0; i < text.Length; i += max)
        {
            var chunk = text.Substring(i, Math.Min(max, text.Length - i));
            try
            {
                await bot.SendMessage(chatId, chunk, parseMode: ParseMode.Markdown, cancellationToken: ct);
            }
            catch (ApiRequestException apiEx) when (IsFatalApiError(apiEx))
            {
                // Token bị revoke / bot bị ban → không retry, throw để caller biết
                throw;
            }
            catch
            {
                // Markdown parse lỗi → fallback gửi plain text
                await bot.SendMessage(chatId, chunk, cancellationToken: ct);
            }
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
        catch
        {
        }
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken ct)
    {
        // 409 Conflict: có instance khác đang dùng cùng token → dừng polling process này
        if (ex.Message.Contains("409: Conflict", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("[TelegramBot] PID {ProcessId}: Another instance is using this bot token. Stop polling in this process.", Environment.ProcessId);
            _pollingCancellationTokenSource?.Cancel();
            return Task.CompletedTask;
        }

        // 401 Unauthorized / 403 Forbidden: token bị revoke hoặc bot bị ban
        // → Dừng polling ngay lập tức, KHÔNG retry để tránh crash service
        if (ex is ApiRequestException { ErrorCode: 401 or 403 } fatalEx)
        {
            _logger.LogCritical(
                "[TelegramBot] PID {ProcessId}: Fatal API error during polling (Code={ErrorCode}): {Message}. "
                + "Bot token may be revoked or bot is banned. Stopping polling permanently. Restart the service after fixing the token.",
                Environment.ProcessId, fatalEx.ErrorCode, fatalEx.Message);
            _pollingCancellationTokenSource?.Cancel();
            return Task.CompletedTask;
        }

        _logger.LogError(ex, "[TelegramBot] Telegram polling error");
        return Task.CompletedTask;
    }
}
