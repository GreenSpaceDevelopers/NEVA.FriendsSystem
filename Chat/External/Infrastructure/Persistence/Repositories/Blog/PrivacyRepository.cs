using Application.Abstractions.Persistence.Repositories.Blog;
using Domain.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Blog;

public class PrivacyRepository(ChatsDbContext dbContext) : IPrivacyRepository
{
    public async Task<UserPrivacySettings?> GetUserPrivacySettingsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<UserPrivacySettings>()
            .FirstOrDefaultAsync(p => p.ChatUserId == userId, cancellationToken);
    }

    public async Task<UserPrivacySettings> CreateDefaultPrivacySettingsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var settings = new UserPrivacySettings
        {
            Id = Guid.NewGuid(),
            ChatUserId = userId,
            FriendsListVisibility = PrivacyLevel.Friends,
            CommentsPermission = PrivacyLevel.Public,
            DirectMessagesPermission = PrivacyLevel.Friends,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await dbContext.Set<UserPrivacySettings>().AddAsync(settings, cancellationToken);
        return settings;
    }

    public async Task UpdateUserPrivacySettingsAsync(UserPrivacySettings settings, CancellationToken cancellationToken = default)
    {
        settings.UpdatedAt = DateTime.UtcNow;
        dbContext.Set<UserPrivacySettings>().Update(settings);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}