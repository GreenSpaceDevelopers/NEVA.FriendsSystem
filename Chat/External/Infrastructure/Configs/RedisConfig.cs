namespace Infrastructure.Configs;

public class RedisConfig
{
    public const string SectionName = "Redis";
    public string ConnectionString { get; set; } = string.Empty;
}