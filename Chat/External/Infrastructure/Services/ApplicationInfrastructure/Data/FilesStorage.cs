using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace Infrastructure.Services.ApplicationInfrastructure.Data;

public class FilesStorage : IFilesStorage
{
    private readonly IMinioClient _minioClient;
    private readonly Infrastructure.Configs.MinioConfig _config;

    public FilesStorage(IOptions<Infrastructure.Configs.MinioConfig> config)
    {
        _config = config.Value;
        
        var minioClientBuilder = new MinioClient()
            .WithEndpoint(_config.Endpoint)
            .WithCredentials(_config.AccessKey, _config.SecretKey);
    
        if (_config.UseSSL)
        {
            minioClientBuilder = minioClientBuilder.WithSSL();
        }
        
        var httpClientHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true,
            MaxConnectionsPerServer = 10,
            UseCookies = false
        };
    
        var httpClient = new HttpClient(httpClientHandler)
        {
            Timeout = TimeSpan.FromSeconds(120)
        };
        
        minioClientBuilder = minioClientBuilder.WithHttpClient(httpClient);
    
        _minioClient = minioClientBuilder.Build();
    }

    public async Task<IOperationResult> UploadAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        var objectName = GetUniqueFileName(fileName);
        
        try
        {
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }
            
            var found = await _minioClient!.BucketExistsAsync(new BucketExistsArgs().WithBucket(_config.BucketName), cancellationToken);
            if (!found)
            {
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_config.BucketName), cancellationToken);
            }

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_config.BucketName)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType("auto");
            
            await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);
            
            var publicEndpoint = !string.IsNullOrEmpty(_config.PublicEndpoint) ? _config.PublicEndpoint : _config.Endpoint;
            var url = $"{(_config.UseSSL ? "https" : "http")}://{publicEndpoint}/{_config.BucketName}/{objectName}";
            return ResultsHelper.Ok(url);
        }
        catch (MinioException ex)
        {
            return ResultsHelper.BadRequest($"File upload failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            return ResultsHelper.BadRequest($"File upload failed: {ex.Message}");
        }
    }
    
    private static string GetUniqueFileName(string fileName)
    {
        return $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}