using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace Infrastructure.Services.ApplicationInfrastructure.Data;

public class FilesStorage : IFilesStorage
{
    private readonly IMinioClient _minioClient;
    private readonly Infrastructure.Configs.MinioConfig _config;

    public FilesStorage(IOptions<Infrastructure.Configs.MinioConfig> config)
    {
        _config = config.Value;
        _minioClient = new MinioClient()
            .WithEndpoint(_config.Endpoint)
            .WithCredentials(_config.AccessKey, _config.SecretKey);
        if (_config.UseSSL)
        {
            _minioClient = _minioClient.WithSSL();
        }

        _minioClient = _minioClient.Build();
    }

    public async Task<IOperationResult> UploadAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            var objectName = $"{Guid.NewGuid()}/{fileName}";

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_config.BucketName)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(GetContentType(fileName));

            await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);

            var url = $"{(_config.UseSSL ? "https" : "http")}://{_config.Endpoint}/{_config.BucketName}/{objectName}";
            return ResultsHelper.Ok(url);
        }
        catch (Exception ex)
        {
            return ResultsHelper.BadRequest($"Failed to upload file: {ex.Message}");
        }
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