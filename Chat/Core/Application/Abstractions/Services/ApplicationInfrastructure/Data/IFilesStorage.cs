using Application.Abstractions.Services.ApplicationInfrastructure.Results;

namespace Application.Abstractions.Services.ApplicationInfrastructure.Data;

public interface IFilesStorage
{
    public Task<IOperationResult> UploadAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
}