using Application.Common.Models;
using Application.Dtos.Requests.Shared;
using Domain.Models.Messaging;
using Domain.Models.Media;

namespace Application.Abstractions.Persistence.Repositories.Messaging;

public record ChatWithUnreadCount(Chat Chat, int UnreadCount);

public interface IChatsRepository : IBaseRepository<Chat>
{
    public Task<PagedList<ChatWithUnreadCount>> GetUserChatsWithUnreadCountAsync(
        Guid userId,
        PageSettings pageSettings,
        string? searchQuery = null,
        CancellationToken cancellationToken = default);
    
    public Task<Message?> GetLastMessageInChatAsync(Guid chatId, CancellationToken cancellationToken = default);
    
    public Task<Guid[]> GetUserIdsFromChatNoTrackingAsync(string messageChatId);
    
    public Task<List<Message>> GetMessagesByChatIdNoTrackingAsync(Guid chatId, int take, int skip, CancellationToken cancellationToken = default);
    
    Task<Chat?> GetByIdWithUsersAsync(Guid chatId, CancellationToken cancellationToken = default);

    Task<Chat?> GetChatPreviewAsync(Guid chatId, CancellationToken cancellationToken = default);
    
    Task AddPictureAsync(Picture picture, CancellationToken cancellationToken = default);
    
    Task<List<Guid>> GetUserChatIdsAsync(Guid userId, CancellationToken cancellationToken = default);
}