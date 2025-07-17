using Application.Abstractions.Services.ApplicationInfrastructure.Results;

namespace Application.Abstractions.Services.ApplicationInfrastructure.Data;

public interface IFilesStorage
{
    public Task<IOperationResult> UploadAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
    public Task DeleteFileAsync(string fileName, string? bucketName = null, CancellationToken cancellationToken = default);
}

public interface IAvatarStorage
{
    public Task<IOperationResult> UploadAvatarAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
}