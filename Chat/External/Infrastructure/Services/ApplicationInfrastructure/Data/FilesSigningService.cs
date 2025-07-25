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
        if (urlOrObjectName.Contains("avatars.steamstatic.com", StringComparison.OrdinalIgnoreCase) ||
            urlOrObjectName.StartsWith("http://") || 
            urlOrObjectName.StartsWith("https://"))
        {
            return urlOrObjectName;
        }

        return await GetSignedUrlForObjectAsync(urlOrObjectName, _config.BucketName, cancellationToken);
    }

    public async Task<string> GetSignedUrlForObjectAsync(string objectName, string bucketName, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(objectName))
            {
                return string.Empty;
            }
            
            if (objectName.Contains("avatars.steamstatic.com", StringComparison.OrdinalIgnoreCase) ||
                objectName.StartsWith("http://") || 
                objectName.StartsWith("https://"))
            {
                return objectName;
            }

            var presignedGetObjectArgs = new PresignedGetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithExpiry(60 * 60 * 24);

            var signedUrl = await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs);
            return signedUrl;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating signed URL for {objectName} in {bucketName}: {ex.Message}");
            return string.Empty;
        }
    }

    public string BuildFullUrl(string objectName, string bucketName)
    {
        var endpoint = _config.PublicEndpoint;
        return $"{(_config.UseSSL ? "https" : "http")}://{endpoint}/{bucketName}/{objectName}";
    }
}