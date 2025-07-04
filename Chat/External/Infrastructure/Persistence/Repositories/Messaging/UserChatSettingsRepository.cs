using Application.Abstractions.Persistence.Repositories.Messaging;
using Domain.Models.Messaging;
using Domain.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Messaging;

public class UserChatSettingsRepository(ChatsDbContext dbContext) : IUserChatSettingsRepository
{
    public Task<UserChatSettings?> GetByUserAndChatAsync(Guid userId, Guid chatId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<UserChatSettings>()
            .Include(ucs => ucs.DisabledUsers)
            .FirstOrDefaultAsync(ucs => ucs.UserId == userId && ucs.ChatId == chatId, cancellationToken);
    }

    public async Task<UserChatSettings> GetByUserAndChatOrCreateAsync(Guid userId, Guid chatId, CancellationToken cancellationToken = default)
    {
        var settings = await GetByUserAndChatAsync(userId, chatId, cancellationToken);
        
        if (settings == null)
        {
            settings = new UserChatSettings
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ChatId = chatId,
                IsMuted = false,
                IsDisabled = false,
                DisabledUsers = []
            };
            
            await dbContext.Set<UserChatSettings>().AddAsync(settings, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        
        return settings;
    }

    public Task<List<UserChatSettings>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<UserChatSettings>()
            .Include(ucs => ucs.DisabledUsers)
            .Where(ucs => ucs.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public void UpdateAsync(UserChatSettings settings, CancellationToken cancellationToken = default)
    {
        dbContext.Set<UserChatSettings>().Update(settings);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
} 