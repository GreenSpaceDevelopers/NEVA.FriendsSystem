using Application.Abstractions.Services.ApplicationInfrastructure.Results;

namespace Application.Abstractions.Services.ApplicationInfrastructure.Data;

public interface IFilesStorage
{
    public Task<IOperationResult> UploadAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
    public Task<IOperationResult> DeleteByFileIdAsync(string fileId, CancellationToken cancellationToken = default);
    public Task<IOperationResult> DeleteBatchByFileIdsAsync(IEnumerable<string> fileIds, CancellationToken cancellationToken = default);
}

public record FileUploadResult(string FileId, string Url);