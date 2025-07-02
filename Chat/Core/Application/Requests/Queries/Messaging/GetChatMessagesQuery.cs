using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Dtos.Requests.Shared;
using Application.Dtos.Responses.Chats;
using Application.Services.ApplicationInfrastructure.Results;

namespace Application.Requests.Queries.Messaging;

public record GetChatMessagesQuery(Guid ChatId, PageSettings PageSettings, bool Desc = true) : IRequest;

public class GetChatMessagesQueryHandler(IMessagesRepository messagesRepository) : IRequestHandler<GetChatMessagesQuery>
{
    public async Task<IOperationResult> HandleAsync(GetChatMessagesQuery request, CancellationToken cancellationToken = default)
    {
        var sortExpressions = new List<Common.Models.SortExpression>
        {
            new()
            {
                PropertyName = nameof(Domain.Models.Messaging.Message.CreatedAt),
                Direction = request.Desc ? Common.Models.SortDirection.Desc : Common.Models.SortDirection.Asc
            }
        };

        var pagedMessages = await messagesRepository.GetChatMessagesPagedAsync(
            request.ChatId,
            request.PageSettings,
            sortExpressions,
            cancellationToken);

        var pagedResult = pagedMessages.Map(m => new MessageDto(
            m.Id,
            m.ChatId,
            m.SenderId,
            m.Sender.Username,
            m.Sender.Avatar?.Url,
            m.Content,
            m.Attachment?.Url,
            m.CreatedAt,
            m.Replies.Count,
            m.Reactions.Count
        ));

        return ResultsHelper.Ok(pagedResult);
    }
} 