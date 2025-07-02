using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using FluentValidation;

namespace Application.Requests.Commands.Friends;

public record RemoveFromBlockList(Guid UserId, Guid UnblockedUserId) : IRequest;

public class RemoveFromBlockListHandler(IChatUsersRepository chatUsersRepository) : IRequestHandler<RemoveFromBlockList>
{
    public async Task<IOperationResult> HandleAsync(RemoveFromBlockList request, CancellationToken cancellationToken = default)
    {
        var user = await chatUsersRepository.GetByIdWithBlockerUsersAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return ResultsHelper.NotFound("User not found");
        }

        var unblockedUser = await chatUsersRepository.GetByIdAsync(request.UnblockedUserId, cancellationToken);
        if (unblockedUser is null)
        {
            return ResultsHelper.NotFound("Unblocked user not found");
        }

        if (!user.BlockedUsers.Contains(unblockedUser))
        {
            return ResultsHelper.BadRequest("User is not blocked");
        }

        user.BlockedUsers.Remove(unblockedUser);
        await chatUsersRepository.SaveChangesAsync(cancellationToken);

        return ResultsHelper.NoContent();
    }
}

public class RemoveFromBlockListValidator : AbstractValidator<RemoveFromBlockList>
{
    public RemoveFromBlockListValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.UnblockedUserId)
            .NotEmpty().WithMessage("UnblockedUserId is required.");

        RuleFor(x => x)
            .Must(x => x.UserId != x.UnblockedUserId)
            .WithMessage("UserId and UnblockedUserId must be different.");
    }
}