using Application.Dtos.Requests.Shared;
using Domain.Models.Messaging;

namespace Application.Abstractions.Persistence.Repositories.Messaging;

public interface IChatsRepository : IBaseRepository<Chat>
{
    public Task<List<Chat>> GetUserChatsNoTrackingAsync(Guid userId, PageSettings pageSettings, CancellationToken cancellationToken = default);
    public Task<Guid[]> GetUserIdsFromChatNoTrackingAsync(string messageChatId);
}