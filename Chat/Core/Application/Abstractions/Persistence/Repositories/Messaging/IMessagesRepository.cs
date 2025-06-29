using Application.Dtos.Requests.Shared;
using Domain.Models.Messaging;

namespace Application.Abstractions.Persistence.Repositories.Messaging;

public interface IMessagesRepository
{
    Task<List<Message>> GetChatMessagesAsync(Guid chatId, PageSettings pageSettings, CancellationToken cancellationToken = default);
    Task<List<Message>> GetChatMessagesDescAsync(Guid chatId, PageSettings pageSettings, CancellationToken cancellationToken = default);
} 