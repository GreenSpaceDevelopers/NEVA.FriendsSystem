using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Services.ApplicationInfrastructure.Data;

public class MinioInitializer : IHostedService
{
    private readonly IMinioClient _minioClient;
    private readonly Infrastructure.Configs.MinioConfig _config;

    public MinioInitializer(IOptions<Infrastructure.Configs.MinioConfig> config)
    {
        _config = config.Value;
        _minioClient = new MinioClient()
            .WithEndpoint(_config.Endpoint)
            .WithCredentials(_config.AccessKey, _config.SecretKey);
        if (_config.UseSSL)
        {
            _minioClient = _minioClient.WithSSL();
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var bucketExists = await _minioClient.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(_config.BucketName),
                cancellationToken);

            if (!bucketExists)
            {
                await _minioClient.MakeBucketAsync(
                    new MakeBucketArgs().WithBucket(_config.BucketName),
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail startup
            Console.WriteLine($"Failed to initialize MinIO bucket: {ex.Message}");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}