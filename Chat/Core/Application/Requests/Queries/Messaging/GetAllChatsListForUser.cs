using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Common.Mappers;
using Application.Common.Models;
using Application.Dtos.Requests.Shared;
using Application.Dtos.Responses.Chats;
using Application.Services.ApplicationInfrastructure.Results;

namespace Application.Requests.Queries.Messaging;

public record GetAllChatsForUserQuery(Guid UserId, PageSettings PageSettings, string? SearchQuery = null) : IRequest;

public class GetAllChatsForUserQueryHandler(IChatUsersRepository chatUsersRepository, IChatsRepository chatsRepository, IUserChatSettingsRepository userChatSettingsRepository, IFilesSigningService filesSigningService) : IRequestHandler<GetAllChatsForUserQuery>
{
    public async Task<IOperationResult> HandleAsync(GetAllChatsForUserQuery request, CancellationToken cancellationToken = default)
    {
        var user = await chatUsersRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user is null)
        {
            return ResultsHelper.NotFound("Пользователь не найден");
        }

        var chatsWithUnreadCount = await chatsRepository.GetUserChatsWithUnreadCountAsync(
            request.UserId, 
            request.PageSettings, 
            request.SearchQuery, 
            cancellationToken);

        var userSettings = await userChatSettingsRepository.GetByUserAsync(request.UserId, cancellationToken);
        var mutedDict = userSettings
            .GroupBy(s => s.ChatId)
            .ToDictionary(g => g.Key, g => g.First().IsMuted);

        var chatDtos = new List<UserChatListItemDto>();
        foreach (var chatWithUnreadCount in chatsWithUnreadCount.Data)
        {
            var isMuted = mutedDict.TryGetValue(chatWithUnreadCount.Chat.Id, out var m) && m;
            var chatDto = await chatWithUnreadCount.ToUserChatListItemDtoAsync(request.UserId, filesSigningService, isMuted, cancellationToken);
            chatDtos.Add(chatDto);
        }

        var result = new PagedList<UserChatListItemDto>
        {
            Data = chatDtos,
            TotalCount = chatsWithUnreadCount.TotalCount
        };

        return ResultsHelper.Ok(result);
    }
}