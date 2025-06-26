using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Infrastructure.Configs;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.ApplicationInfrastructure.Data;

public class FilesSigningService : IFilesSigningService
{
    private readonly MinioConfig _minioConfig;

    public FilesSigningService(IOptions<MinioConfig> minioConfig)
    {
        _minioConfig = minioConfig.Value;
    }

    public Task<string> GetSignedUrlAsync(string unsignedUrl, CancellationToken cancellationToken = default)
    {
        var signedUrl = $"{_minioConfig.Endpoint}/{_minioConfig.BucketName}/{unsignedUrl}";
        return Task.FromResult(signedUrl);
    }
} 