using Application.Common.Models;
using Application.Dtos.Requests.Shared;
using Domain.Models.Messaging;

namespace Application.Abstractions.Persistence.Repositories.Messaging;

public interface IMessagesRepository : IBaseRepository<Message>
{
    Task<PagedList<Message>> GetChatMessagesPagedAsync(
        Guid chatId,
        PageSettings pageSettings,
        List<SortExpression>? sortExpressions,
        CancellationToken cancellationToken = default);

    Task<PagedList<Message>> GetUnreadMessagesPagedAsync(
        Guid chatId,
        Guid userId,
        PageSettings pageSettings,
        CancellationToken cancellationToken = default);
} 