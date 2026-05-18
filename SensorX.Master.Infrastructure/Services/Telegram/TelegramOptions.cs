namespace SensorX.Master.Infrastructure.Services.Telegram;

public class TelegramOptions
{
    public const string SectionName = "Telegram";
    public string Token { get; set; } = string.Empty;
}
