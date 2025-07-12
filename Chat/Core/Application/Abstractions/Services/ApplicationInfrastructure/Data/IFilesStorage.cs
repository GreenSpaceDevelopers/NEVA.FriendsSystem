using Application.Abstractions.Services.ApplicationInfrastructure.Results;

namespace Application.Abstractions.Services.ApplicationInfrastructure.Data;

public interface IFilesStorage
{
    public Task<IOperationResult> UploadAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
    public Task<IOperationResult> DeleteAsync(string fileUrl, CancellationToken cancellationToken = default);
    public Task<IOperationResult> DeleteBatchAsync(IEnumerable<string> fileUrls, CancellationToken cancellationToken = default);
}