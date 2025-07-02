using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Dtos.Responses.Profile;
using Application.Services.ApplicationInfrastructure.Results;
using FluentValidation;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Requests.Commands.Profile;

/// <summary>
/// Запрос на валидацию имени пользователя
/// </summary>
[SwaggerSchema(Description = "Запрос для проверки доступности имени пользователя")]
public record ValidateUsernameRequest(
    [SwaggerSchema(Description = "Имя пользователя для проверки (от 3 до 50 символов)")]
    string Username) : IRequest;

public class ValidateUsernameRequestHandler(IChatUsersRepository chatUsersRepository) : IRequestHandler<ValidateUsernameRequest>
{
    public async Task<IOperationResult> HandleAsync(ValidateUsernameRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Username.Length < 3)
        {
            return ResultsHelper.Ok(new ProfileValidationDto(false, "Username must be at least 3 characters long"));
        }

        var isUnique = await chatUsersRepository.IsUsernameUniqueAsync(request.Username, cancellationToken);

        if (!isUnique)
        {
            return ResultsHelper.Ok(new ProfileValidationDto(false, "Username is already taken"));
        }

        return ResultsHelper.Ok(new ProfileValidationDto(true, null));
    }
}

public class ValidateUsernameRequestValidator : AbstractValidator<ValidateUsernameRequest>
{
    public ValidateUsernameRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters long.")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters.");
    }
}