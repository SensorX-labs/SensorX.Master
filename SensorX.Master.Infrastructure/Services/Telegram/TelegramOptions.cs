namespace SensorX.Master.Infrastructure.Services.Telegram;

public class TelegramOptions
{
    public const string SectionName = "Telegram";
    public bool Enabled { get; set; }
    public string Token { get; set; } = string.Empty;
    public string AdminId { get; set; } = string.Empty;
    public string SetupSecret { get; set; } = "TECHLEAD_2026";
    public string NineRouterUrl { get; set; } = "http://localhost:3001";
    public string NineRouterModel { get; set; } = "auto";
}
