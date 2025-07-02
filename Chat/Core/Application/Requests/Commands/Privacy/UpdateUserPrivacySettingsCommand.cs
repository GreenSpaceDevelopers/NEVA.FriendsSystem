using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using Domain.Models.Users;
using FluentValidation;

namespace Application.Requests.Commands.Privacy;

public record UpdateUserPrivacySettingsCommand(
    Guid UserId,
    PrivacyLevel? FriendsListVisibility,
    PrivacyLevel? CommentsPermission,
    PrivacyLevel? DirectMessagesPermission
) : IRequest;

public class UpdateUserPrivacySettingsCommandHandler(IPrivacyRepository privacyRepository) : IRequestHandler<UpdateUserPrivacySettingsCommand>
{
    public async Task<IOperationResult> HandleAsync(UpdateUserPrivacySettingsCommand request, CancellationToken cancellationToken = default)
    {
        var settings = await privacyRepository.GetUserPrivacySettingsAsync(request.UserId, cancellationToken) ?? await privacyRepository.CreateDefaultPrivacySettingsAsync(request.UserId, cancellationToken);

        if (request.FriendsListVisibility.HasValue)
            settings.FriendsListVisibility = request.FriendsListVisibility.Value;
        
        if (request.CommentsPermission.HasValue)
            settings.CommentsPermission = request.CommentsPermission.Value;
        
        if (request.DirectMessagesPermission.HasValue)
            settings.DirectMessagesPermission = request.DirectMessagesPermission.Value;

        await privacyRepository.UpdateUserPrivacySettingsAsync(settings, cancellationToken);

        return ResultsHelper.NoContent();
    }
}

public class UpdateUserPrivacySettingsCommandValidator : AbstractValidator<UpdateUserPrivacySettingsCommand>
{
    public UpdateUserPrivacySettingsCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        When(x => x.FriendsListVisibility.HasValue, () =>
        {
            RuleFor(x => x.FriendsListVisibility!.Value)
                .IsInEnum().WithMessage("Invalid FriendsListVisibility value.");
        });

        When(x => x.CommentsPermission.HasValue, () =>
        {
            RuleFor(x => x.CommentsPermission!.Value)
                .IsInEnum().WithMessage("Invalid CommentsPermission value.");
        });

        When(x => x.DirectMessagesPermission.HasValue, () =>
        {
            RuleFor(x => x.DirectMessagesPermission!.Value)
                .IsInEnum().WithMessage("Invalid DirectMessagesPermission value.");
        });
    }
} 