using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Infrastructure.Configs;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.ApplicationInfrastructure.Data;

public class FilesSigningService(IOptions<MinioConfig> minioConfig) : IFilesSigningService
{

    public Task<string> GetSignedUrlAsync(string unsignedUrl, CancellationToken cancellationToken = default)
    {
        var signedUrl = $"{minioConfig.Value.Endpoint}/{minioConfig.Value.BucketName}/{unsignedUrl}";
        return Task.FromResult(signedUrl);
    }
}