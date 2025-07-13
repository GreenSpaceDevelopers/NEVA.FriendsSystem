using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using FluentValidation;

namespace Application.Requests.Commands.Profile;

public record DeleteCoverCommand(Guid UserId) : IRequest;

public class DeleteCoverCommandHandler(IChatUsersRepository usersRepository, IFilesStorage filesStorage) : IRequestHandler<DeleteCoverCommand>
{
    public async Task<IOperationResult> HandleAsync(DeleteCoverCommand request, CancellationToken cancellationToken = default)
    {
        var user = await usersRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return ResultsHelper.NotFound("User not found");
        }

        if (user.Cover != null && !string.IsNullOrEmpty(user.Cover.FileId))
        {
            await filesStorage.DeleteByFileIdAsync(user.Cover.FileId, cancellationToken);
        }

        user.Cover = null;
        user.CoverId = null;

        usersRepository.Update(user);
        await usersRepository.SaveChangesAsync(cancellationToken);

        return ResultsHelper.NoContent();
    }
}

public class DeleteCoverCommandValidator : AbstractValidator<DeleteCoverCommand>
{
    public DeleteCoverCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.")
            .Must(id => id != Guid.Empty).WithMessage("UserId must not be empty.");
    }
} 