using Application.Common.Models;
using Application.Dtos.Requests.Shared;
using Domain.Models.Messaging;

namespace Application.Abstractions.Persistence.Repositories.Messaging;

public interface IChatsRepository : IBaseRepository<Chat>
{
    public Task<PagedList<Chat>> GetUserChatsNoTrackingAsync(Guid userId, PageSettings pageSettings, CancellationToken cancellationToken = default);
    public Task<Guid[]> GetUserIdsFromChatNoTrackingAsync(string messageChatId);
    public Task<List<Message>> GetMessagesByChatIdNoTrackingAsync(Guid chatId, int take, int skip, CancellationToken cancellationToken = default);
}