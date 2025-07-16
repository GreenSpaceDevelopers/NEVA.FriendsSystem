using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using MinioConfig = Infrastructure.Configs.MinioConfig;

namespace Infrastructure.Services.ApplicationInfrastructure.Data;

public class FilesSigningService : IFilesSigningService
{
    private readonly IMinioClient _minioClient;
    private readonly MinioConfig _config;

    public FilesSigningService(IOptions<MinioConfig> minioConfig)
    {
        _config = minioConfig.Value;
        _minioClient = new MinioClient()
            .WithEndpoint(_config.PublicEndpoint)
            .WithCredentials(_config.AccessKey, _config.SecretKey);
        if (_config.UseSSL)
        {
            _minioClient = _minioClient.WithSSL();
        }
        _minioClient = _minioClient.Build();
    }

    public async Task<string> GetSignedUrlAsync(string urlOrObjectName, CancellationToken cancellationToken = default)
    {
        if (urlOrObjectName.Contains("avatars.steamstatic.com", StringComparison.OrdinalIgnoreCase))
        {
            return urlOrObjectName;
        }

        try
        {
            var objectName = ExtractObjectNameFromUrl(urlOrObjectName);
            
            if (string.IsNullOrEmpty(objectName))
            {
                return string.Empty;
            }

            var presignedGetObjectArgs = new PresignedGetObjectArgs()
                .WithBucket(_config.BucketName)
                .WithObject(objectName)
                .WithExpiry(60 * 60 * 24);

            var signedUrl = await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs);
            return signedUrl;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    private string ExtractObjectNameFromUrl(string urlOrObjectName)
    {
        if (!urlOrObjectName.StartsWith("http://") && !urlOrObjectName.StartsWith("https://"))
        {
            return urlOrObjectName;
        }

        try
        {
            var schemeIndex = urlOrObjectName.IndexOf("://");
            if (schemeIndex == -1) return urlOrObjectName;
            
            var urlWithoutScheme = urlOrObjectName[(schemeIndex + 3)..];
            
            var firstSlashIndex = urlWithoutScheme.IndexOf('/');
            if (firstSlashIndex == -1) return string.Empty;
            
            var pathPart = urlWithoutScheme[firstSlashIndex..];
            
            var bucketPrefix = $"/{_config.BucketName}/";
            var bucketIndex = pathPart.IndexOf(bucketPrefix, StringComparison.OrdinalIgnoreCase);
            
            if (bucketIndex == -1)
            {
                return pathPart.TrimStart('/');
            }

            var objectName = pathPart[(bucketIndex + bucketPrefix.Length)..];
            
            return Uri.UnescapeDataString(objectName);
        }
        catch
        {
            return urlOrObjectName;
        }
    }
}