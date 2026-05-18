using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SensorX.Master.Infrastructure.Services.Telegram;

public class TelegramBotBackgroundService : BackgroundService
{
    private readonly ILogger<TelegramBotBackgroundService> _logger;
    private readonly ITelegramBotClient _botClient;

    public TelegramBotBackgroundService(
        ILogger<TelegramBotBackgroundService> logger,
        ITelegramBotClient botClient)
    {
        _logger = logger;
        _botClient = botClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var me = await _botClient.GetMe(stoppingToken);
            _logger.LogInformation("Telegram Bot @{BotName} is starting...", me.Username);

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // Receive all update types
            };

            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                errorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: stoppingToken
            );

            _logger.LogInformation("Telegram Bot @{BotName} is running.", me.Username);

            // Keep the service alive until stoppingToken is cancelled
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Telegram Bot service is stopping.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Telegram Bot");
        }
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message)
            return;

        if (message.Text is not { } messageText)
            return;

        var chatId = message.Chat.Id;
        var username = message.From?.Username ?? "Unknown";
        var firstName = message.From?.FirstName ?? "";

        _logger.LogInformation("User {FirstName} (@{Username}) sent: {MessageText}", firstName, username, messageText);

        // Echo back to user
        await botClient.SendMessage(
            chatId: chatId,
            text: $"Bot đã nhận tin nhắn từ bạn: {messageText}",
            cancellationToken: cancellationToken);
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Telegram Bot Polling Error");
        return Task.CompletedTask;
    }
}
