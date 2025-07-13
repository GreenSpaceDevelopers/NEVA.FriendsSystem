using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using FluentValidation;

namespace Application.Requests.Commands.Profile;

public record DeleteAvatarCommand(Guid UserId) : IRequest;

public class DeleteAvatarCommandHandler(IChatUsersRepository usersRepository, IFilesStorage filesStorage) : IRequestHandler<DeleteAvatarCommand>
{
    public async Task<IOperationResult> HandleAsync(DeleteAvatarCommand request, CancellationToken cancellationToken = default)
    {
        var user = await usersRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return ResultsHelper.NotFound("User not found");
        }

        if (user.Avatar != null && !string.IsNullOrEmpty(user.Avatar.FileId))
        {
            await filesStorage.DeleteByFileIdAsync(user.Avatar.FileId, cancellationToken);
        }

        user.Avatar = null;
        user.AvatarId = null;

        usersRepository.Update(user);
        await usersRepository.SaveChangesAsync(cancellationToken);

        return ResultsHelper.NoContent();
    }
}

public class DeleteAvatarCommandValidator : AbstractValidator<DeleteAvatarCommand>
{
    public DeleteAvatarCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.")
            .Must(id => id != Guid.Empty).WithMessage("UserId must not be empty.");
    }
} 