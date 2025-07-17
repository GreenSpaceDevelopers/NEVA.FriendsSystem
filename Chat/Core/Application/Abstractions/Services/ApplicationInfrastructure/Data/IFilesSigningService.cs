namespace Application.Abstractions.Services.ApplicationInfrastructure.Data;

public interface IFilesSigningService
{
    public Task<string> GetSignedUrlAsync(string unsignedUrl, CancellationToken cancellationToken = default);
    public Task<string> GetSignedUrlForObjectAsync(string objectName, string bucketName, CancellationToken cancellationToken = default);
    public string BuildFullUrl(string objectName, string bucketName);
}