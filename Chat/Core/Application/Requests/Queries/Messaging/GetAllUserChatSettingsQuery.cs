using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Dtos.Responses.Messaging;
using Application.Services.ApplicationInfrastructure.Results;
using FluentValidation;

namespace Application.Requests.Queries.Messaging;

public record GetAllUserChatSettingsQuery(Guid UserId) : IRequest;

public class GetAllUserChatSettingsQueryHandler(IUserChatSettingsRepository userChatSettingsRepository) 
    : IRequestHandler<GetAllUserChatSettingsQuery>
{
    public async Task<IOperationResult> HandleAsync(GetAllUserChatSettingsQuery query, CancellationToken cancellationToken = default)
    {
        var settings = await userChatSettingsRepository.GetByUserAsync(query.UserId, cancellationToken);
        
        var response = settings.Select(s => new UserChatSettingsDto(
            s.Id,
            s.UserId,
            s.ChatId,
            s.IsMuted,
            s.IsDisabled,
            s.LastReadMessageId,
            s.DisabledUsers?.Select(u => u.Id).ToList() ?? new List<Guid>()
        )).ToList();
        
        return ResultsHelper.Ok(response);
    }
}

public class GetAllUserChatSettingsQueryValidator : AbstractValidator<GetAllUserChatSettingsQuery>
{
    public GetAllUserChatSettingsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("ID пользователя обязателен");
    }
} 