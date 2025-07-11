using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Dtos.Responses.Profile;
using Application.Services.ApplicationInfrastructure.Results;
using FluentValidation;

namespace Application.Requests.Commands.Profile;

public record ValidatePersonalLinkRequest(string PersonalLink) : IRequest;

public class ValidatePersonalLinkRequestHandler(IChatUsersRepository chatUsersRepository) : IRequestHandler<ValidatePersonalLinkRequest>
{
    public async Task<IOperationResult> HandleAsync(ValidatePersonalLinkRequest request, CancellationToken cancellationToken = default)
    {
        var isAvailable = await chatUsersRepository.IsPersonalLinkUniqueAsync(request.PersonalLink, cancellationToken);
        
        string? errorMessage = null;
        if (!isAvailable)
        {
            errorMessage = "Personal link is already taken";
        }

        var validationResult = new ProfileValidationDto(isAvailable, errorMessage);
        return ResultsHelper.Ok(validationResult);
    }
}

public class ValidatePersonalLinkRequestValidator : AbstractValidator<ValidatePersonalLinkRequest>
{
    public ValidatePersonalLinkRequestValidator()
    {
        RuleFor(x => x.PersonalLink)
            .NotEmpty().WithMessage("Personal link is required.")
            .MinimumLength(3).WithMessage("Personal link must be at least 3 characters long.")
            .MaximumLength(50).WithMessage("Personal link must not exceed 50 characters.")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Personal link can only contain letters, numbers, and underscores.");
    }
} 