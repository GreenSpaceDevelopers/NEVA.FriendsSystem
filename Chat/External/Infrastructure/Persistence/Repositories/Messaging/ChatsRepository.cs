using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Dtos.Requests.Shared;
using Domain.Models.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Messaging;

public class ChatsRepository(ChatsDbContext dbContext) : BaseRepository<Chat>(dbContext), IChatsRepository
{
    public async Task<List<Chat>> GetUserChatsNoTrackingAsync(Guid userId, PageSettings pageSettings, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Chat>()
            .AsNoTracking()
            .Include(c => c.ChatPicture)
            .Include(c => c.Users)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .ThenInclude(m => m.Sender)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .ThenInclude(m => m.Attachment)
            .Where(c => c.Users.Any(u => u.Id == userId))
            .OrderByDescending(c => c.LastMessageDate)
            .Skip((pageSettings.PageNumber - 1) * pageSettings.PageSize)
            .Take(pageSettings.PageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid[]> GetUserIdsFromChatNoTrackingAsync(string messageChatId)
    {
        return await dbContext.Set<Chat>()
            .AsNoTracking()
            .Where(c => c.Id.ToString() == messageChatId)
            .SelectMany(c => c.Users)
            .Select(u => u.Id)
            .ToArrayAsync();
    }
}