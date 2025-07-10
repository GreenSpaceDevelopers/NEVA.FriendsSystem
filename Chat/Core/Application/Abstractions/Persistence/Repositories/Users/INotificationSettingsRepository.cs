using Domain.Models.Service;

namespace Application.Abstractions.Persistence.Repositories.Users;

public interface INotificationSettingsRepository : IBaseRepository<NotificationSettings>
{
    public Task<NotificationSettings> GetByUserIdOrCreateAsync(Guid userId, CancellationToken cancellationToken = default);
} 