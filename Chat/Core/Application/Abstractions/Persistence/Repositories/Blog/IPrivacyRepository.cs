using Domain.Models.Users;

namespace Application.Abstractions.Persistence.Repositories.Blog;

public interface IPrivacyRepository
{
    Task<UserPrivacySettings?> GetUserPrivacySettingsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserPrivacySettings> CreateDefaultPrivacySettingsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task UpdateUserPrivacySettingsAsync(UserPrivacySettings settings, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}