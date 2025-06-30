using Application.Abstractions.Persistence.Repositories.Users;
using Domain.Models.Service;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Users;

public class NotificationSettingsRepository(ChatsDbContext dbContext) : BaseRepository<NotificationSettings>(dbContext), INotificationSettingsRepository
{
    private readonly ChatsDbContext _dbContext = dbContext;

    public async Task<NotificationSettings?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<NotificationSettings>()
            .FirstOrDefaultAsync(ns => ns.UserId == userId, cancellationToken);
    }

    public async Task<NotificationSettings> GetByUserIdOrCreateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var settings = await GetByUserIdAsync(userId, cancellationToken);
        
        if (settings == null)
        {
            settings = new NotificationSettings
            {
                Id = userId,
                UserId = userId,
                IsEnabled = true,
                IsTelegramEnabled = true,
                IsEmailEnabled = true,
                IsPushEnabled = true,
                NewTournamentPosts = true,
                TournamentUpdates = true,
                TournamentTeamInvites = true,
                TeamChanged = true,
                TeamInvites = true,
                TournamentInvites = true,
                AdminRoleAssigned = true,
                TournamentLobbyInvites = true,
                NewTournamentStep = true,
                TournamentStarted = true,
                NewFriendRequest = true,
                NewMessage = true,
                NewPostComment = true,
                NewCommentReply = true,
                NewCommentReaction = true
            };
            
            await _dbContext.Set<NotificationSettings>().AddAsync(settings, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        
        return settings;
    }
} 