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
    
    public async Task<IOperationResult> DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var objectName = ExtractObjectNameFromUrl(fileUrl);
            
            if (string.IsNullOrEmpty(objectName))
            {
                return ResultsHelper.BadRequest("Invalid file URL");
            }

            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(_config.BucketName)
                .WithObject(objectName);
            
            await _minioClient.RemoveObjectAsync(removeObjectArgs, cancellationToken);
            
            return ResultsHelper.Ok("File deleted successfully");
        }
        catch (MinioException ex)
        {
            return ResultsHelper.BadRequest($"File deletion failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            return ResultsHelper.BadRequest($"File deletion failed: {ex.Message}");
        }
    }

    public async Task<IOperationResult> DeleteBatchAsync(IEnumerable<string> fileUrls, CancellationToken cancellationToken = default)
    {
        try
        {
            var objectNames = fileUrls
                .Select(ExtractObjectNameFromUrl)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();
            
            if (objectNames.Count == 0)
            {
                return ResultsHelper.Ok("No valid files to delete");
            }

            var removeObjectsArgs = new RemoveObjectsArgs()
                .WithBucket(_config.BucketName)
                .WithObjects(objectNames);
            
            await _minioClient.RemoveObjectsAsync(removeObjectsArgs, cancellationToken);
            
            return ResultsHelper.Ok($"Successfully deleted {objectNames.Count} files");
        }
        catch (MinioException ex)
        {
            return ResultsHelper.BadRequest($"Batch file deletion failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            return ResultsHelper.BadRequest($"Batch file deletion failed: {ex.Message}");
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

    private static string ExtractObjectNameFromUrl(string fileUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(fileUrl))
                return string.Empty;

            var uri = new Uri(fileUrl);
            var pathSegments = uri.AbsolutePath.Trim('/').Split('/');
            
            return pathSegments.Length >= 2 ? string.Join("/", pathSegments.Skip(1)) : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}