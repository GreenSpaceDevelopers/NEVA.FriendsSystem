namespace Infrastructure.Configs;

public class TelegramConfig
{
    public const string SectionName = "Telegram";
    public string BotToken { get; set; } = string.Empty;
    public string ErrorsChatId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = "NEVA Chat";
} 