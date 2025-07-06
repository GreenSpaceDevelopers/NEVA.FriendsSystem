using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Common.Models;
using Application.Dtos.Requests.Shared;
using Domain.Models.Messaging;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Messaging;

public class MessagesRepository(ChatsDbContext dbContext) : BaseRepository<Message>(dbContext), IMessagesRepository
{
    public Task<PagedList<Message>> GetChatMessagesPagedAsync(
        Guid chatId,
        PageSettings pageSettings,
        List<SortExpression>? sortExpressions,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Set<Message>()
            .AsNoTracking()
            .Include(m => m.Sender)
            .Include(m => m.Attachment)
            .Include(m => m.Replies)
            .Include(m => m.Reactions)
            .Where(m => m.ChatId == chatId);

        return query.ToPagedList(sortExpressions, pageSettings.Skip, pageSettings.Take, cancellationToken);
    }

    public Task<PagedList<Message>> GetUnreadMessagesPagedAsync(
        Guid chatId,
        Guid userId,
        PageSettings pageSettings,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Set<Message>()
            .AsNoTracking()
            .Include(m => m.Sender)
            .Include(m => m.Attachment)
            .Include(m => m.Reactions)
            .Where(m => m.ChatId == chatId && m.Reactions.All(r => r.ReactorId != userId));

        return query.ToSearchResultWithoutOrder(pageSettings.Skip, pageSettings.Take, cancellationToken);
    }
} 