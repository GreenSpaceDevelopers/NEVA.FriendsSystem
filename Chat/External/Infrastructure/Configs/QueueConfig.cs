namespace Infrastructure.Configs;

public class QueueConfig
{
    public const string SectionName = "Queue";
    public string Host { get; set; } = string.Empty;
    public ushort Port { get; set; } = 0;
    public string HMACKey { get; set; } = string.Empty;
}