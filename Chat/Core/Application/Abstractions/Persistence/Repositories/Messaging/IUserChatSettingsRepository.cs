using Domain.Models.Messaging;

namespace Application.Abstractions.Persistence.Repositories.Messaging;

public interface IUserChatSettingsRepository
{
    public Task<UserChatSettings?> GetByUserAndChatAsync(Guid userId, Guid chatId, CancellationToken cancellationToken = default);
    public Task<UserChatSettings> GetByUserAndChatOrCreateAsync(Guid userId, Guid chatId, CancellationToken cancellationToken = default);
    public Task<List<UserChatSettings>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    public void UpdateAsync(UserChatSettings settings, CancellationToken cancellationToken = default);
    public Task SaveChangesAsync(CancellationToken cancellationToken = default);
} 