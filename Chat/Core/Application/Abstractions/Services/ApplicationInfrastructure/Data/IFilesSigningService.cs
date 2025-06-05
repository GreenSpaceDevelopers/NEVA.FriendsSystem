namespace Application.Abstractions.Services.ApplicationInfrastructure.Data;

public interface IFilesSigningService
{
    public Task<string> GetSignedUrlAsync(string unsignedUrl, CancellationToken cancellationToken = default);
}