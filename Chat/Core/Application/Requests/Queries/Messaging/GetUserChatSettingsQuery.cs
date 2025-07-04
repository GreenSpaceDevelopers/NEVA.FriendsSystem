using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Dtos.Responses.Messaging;
using Application.Services.ApplicationInfrastructure.Results;
using FluentValidation;

namespace Application.Requests.Queries.Messaging;

public record GetUserChatSettingsQuery(Guid UserId, Guid ChatId) : IRequest;

public class GetUserChatSettingsQueryHandler(IUserChatSettingsRepository userChatSettingsRepository) 
    : IRequestHandler<GetUserChatSettingsQuery>
{
    public async Task<IOperationResult> HandleAsync(GetUserChatSettingsQuery query, CancellationToken cancellationToken = default)
    {
        var settings = await userChatSettingsRepository.GetByUserAndChatOrCreateAsync(query.UserId, query.ChatId, cancellationToken);
        
        var response = new UserChatSettingsDto(
            settings.Id,
            settings.UserId,
            settings.ChatId,
            settings.IsMuted,
            settings.IsDisabled,
            settings.DisabledUsers?.Select(u => u.Id).ToList() ?? new List<Guid>()
        );
        
        return ResultsHelper.Ok(response);
    }
}

public class GetUserChatSettingsQueryValidator : AbstractValidator<GetUserChatSettingsQuery>
{
    public GetUserChatSettingsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("ID пользователя обязателен");
            
        RuleFor(x => x.ChatId)
            .NotEmpty()
            .WithMessage("ID чата обязателен");
    }
} 