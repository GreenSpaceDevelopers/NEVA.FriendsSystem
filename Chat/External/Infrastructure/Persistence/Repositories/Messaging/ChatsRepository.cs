using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Common.Models;
using Application.Dtos.Requests.Shared;
using Domain.Models.Messaging;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Messaging;

public class ChatsRepository(ChatsDbContext dbContext) : BaseRepository<Chat>(dbContext), IChatsRepository
{
    public Task<PagedList<Chat>> GetUserChatsNoTrackingAsync(
        Guid userId,
        PageSettings pageSettings,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Chat>()
            .AsNoTracking()
            .Include(c => c.ChatPicture)
            .Include(c => c.Users)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .ThenInclude(m => m.Sender)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .ThenInclude(m => m.Attachment)
            .Where(c => c.Users.Any(u => u.Id == userId))
            .OrderByDescending(c => c.LastMessageDate)
            .ToPagedList(pageSettings, cancellationToken);
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

    public Task<List<Message>> GetMessagesByChatIdNoTrackingAsync(Guid chatId, int take, int skip, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Message>()
            .AsNoTracking()
            .Include(m => m.Sender)
            .Include(m => m.Attachment)
            .Include(m => m.Replies)
            .Include(m => m.Reactions)
            .Where(m => m.ChatId == chatId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<Chat?> GetByIdWithUsersAsync(Guid chatId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Chat>()
            .Include(c => c.Users)
            .FirstOrDefaultAsync(c => c.Id == chatId, cancellationToken);
    }

    public async Task<Chat?> GetChatPreviewAsync(Guid chatId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Chat>()
            .AsNoTracking()
            .Include(c => c.ChatPicture)
            .Include(c => c.Users)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
                .ThenInclude(m => m.Sender)
            .FirstOrDefaultAsync(c => c.Id == chatId, cancellationToken);
    }
}