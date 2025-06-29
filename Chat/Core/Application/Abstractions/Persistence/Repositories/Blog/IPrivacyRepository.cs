using Domain.Models.Users;

namespace Application.Abstractions.Persistence.Repositories.Blog;

public interface IPrivacyRepository
{
    public Task<PrivacySetting> GetPrivacySettingsAsync(Guid userId, CancellationToken cancellationToken = default);
    public Task<List<PrivacySetting>> GetPrivacySettingsAsync(CancellationToken cancellationToken = default);
    public Task AddPrivacySettingsAsync(List<PrivacySetting> settings, CancellationToken cancellationToken = default);
}