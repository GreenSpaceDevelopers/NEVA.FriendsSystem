using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Dtos.Requests.Shared;
using Application.Dtos.Responses.Chats;
using Application.Services.ApplicationInfrastructure.Results;
using FluentValidation;

namespace Application.Requests.Queries.Messaging;

public record GetUnreadMessagesQuery(Guid ChatId, Guid UserId, PageSettings PageSettings) : IRequest;

public class GetUnreadMessagesQueryHandler(IMessagesRepository messagesRepository) : IRequestHandler<GetUnreadMessagesQuery>
{
    public async Task<IOperationResult> HandleAsync(GetUnreadMessagesQuery request, CancellationToken cancellationToken = default)
    {
        var paged = await messagesRepository.GetUnreadMessagesPagedAsync(
            request.ChatId,
            request.UserId,
            request.PageSettings,
            cancellationToken);

        var pagedResult = paged.Map(m => new MessageDto(
            m.Id,
            m.ChatId,
            m.SenderId,
            m.Sender.Username,
            m.Sender.Avatar?.Url,
            m.Content,
            m.Attachment?.Url,
            m.CreatedAt,
            m.Replies.Count,
            m.Reactions.Count));

        return ResultsHelper.Ok(pagedResult);
    }
}

public class GetUnreadMessagesQueryValidator : AbstractValidator<GetUnreadMessagesQuery>
{
    public GetUnreadMessagesQueryValidator()
    {
        RuleFor(x => x.ChatId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.PageSettings).SetValidator(new PageSettingsValidator());
    }
} 