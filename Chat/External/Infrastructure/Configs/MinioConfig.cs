namespace Infrastructure.Configs;

public class MinioConfig
{
    public const string SectionName = "Minio";
    public string Endpoint { get; set; } = string.Empty;
    public string PublicEndpoint { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = "chat-files";
    public string AvatarBucketName { get; set; } = "neva-avatars";
    public bool UseSSL { get; set; } = false;
    public bool IgnoreSSLCertificateErrors { get; set; } = false;
}