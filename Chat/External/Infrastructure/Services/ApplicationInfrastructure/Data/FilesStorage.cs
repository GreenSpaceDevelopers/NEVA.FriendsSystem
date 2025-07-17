using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace Infrastructure.Services.ApplicationInfrastructure.Data;

public class FilesStorage : IFilesStorage, IAvatarStorage
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
        return await UploadToBucketAsync(stream, fileName, _config.BucketName, cancellationToken);
    }

    public async Task<IOperationResult> UploadAvatarAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        return await UploadToBucketAsync(stream, fileName, _config.AvatarBucketName, cancellationToken);
    }

    public async Task<IOperationResult> UploadToBucketAsync(Stream stream, string fileName, string bucketName, CancellationToken cancellationToken = default)
    {
        var objectName = GetUniqueFileName(fileName);
        
        try
        {
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }
            
            var found = await _minioClient!.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName), cancellationToken);
            if (!found)
            {
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName), cancellationToken);
            }

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType("auto");
            
            await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);
            
            return ResultsHelper.Ok(objectName);
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

    public async Task DeleteFileAsync(string fileName, string? bucketName = null, CancellationToken cancellationToken = default)
    {
        var bucket = string.IsNullOrEmpty(bucketName) ? _config.BucketName : bucketName;
        
        try
        {
            await _minioClient!.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(bucket)
                .WithObject(fileName), cancellationToken);
        }
        catch (MinioException ex)
        {
            Console.WriteLine($"Minio error deleting {fileName} from {bucket}: {ex.Message}");
            throw;
        }
    }
    
    private static string GetUniqueFileName(string fileName)
    {
        return $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
    }
}