using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Dtos.Requests.Shared;
using Domain.Models.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Messaging;

public class MessagesRepository(ChatsDbContext dbContext) : IMessagesRepository
{
    public async Task<List<Message>> GetChatMessagesAsync(Guid chatId, PageSettings pageSettings, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Message>()
            .AsNoTracking()
            .Include(m => m.Sender)
            .Include(m => m.Attachment)
            .Include(m => m.Replies)
            .Include(m => m.Reactions)
            .Where(m => m.ChatId == chatId)
            .OrderBy(m => m.CreatedAt)
            .Skip(pageSettings.Skip)
            .Take(pageSettings.Take)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Message>> GetChatMessagesDescAsync(Guid chatId, PageSettings pageSettings, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Message>()
            .AsNoTracking()
            .Include(m => m.Sender)
            .Include(m => m.Attachment)
            .Include(m => m.Replies)
            .Include(m => m.Reactions)
            .Where(m => m.ChatId == chatId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip(pageSettings.Skip)
            .Take(pageSettings.Take)
            .ToListAsync(cancellationToken);
    }
} 