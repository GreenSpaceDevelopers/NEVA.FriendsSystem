using Application.Abstractions.Persistence.Repositories.Blog;
using Domain.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Blog;

public class PrivacyRepository(ChatsDbContext dbContext) : IPrivacyRepository
{
    public async Task<PrivacySetting> GetPrivacySettingsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var setting = await dbContext.Set<PrivacySetting>()
            .FirstOrDefaultAsync(p => p.ChatUserId == userId, cancellationToken);

        if (setting == null)
        {
            throw new InvalidOperationException($"Privacy setting not found for user {userId}");
        }

        return setting;
    }

    public Task<List<PrivacySetting>> GetPrivacySettingsAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Set<PrivacySetting>().ToListAsync(cancellationToken);
    }

    public async Task AddPrivacySettingsAsync(List<PrivacySetting> settings, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<PrivacySetting>().AddRangeAsync(settings, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}