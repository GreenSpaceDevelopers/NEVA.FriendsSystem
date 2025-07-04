using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using FluentValidation;

namespace Application.Requests.Commands.Friends;

public record BlockUserRequest(Guid UserId, Guid BlockedUserId) : IRequest;

public class BlockUserRequestHandler(IChatUsersRepository chatUsersRepository) : IRequestHandler<BlockUserRequest>
{
    public async Task<IOperationResult> HandleAsync(BlockUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await chatUsersRepository.GetByIdWithFriendsAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return ResultsHelper.NotFound("User not found");
        }

        var blockedUser = await chatUsersRepository.GetByIdWithFriendsAsync(request.BlockedUserId, cancellationToken);
        if (blockedUser is null)
        {
            return ResultsHelper.NotFound("Blocked user not found");
        }

        user.Friends.RemoveAll(f => f.Id == blockedUser.Id);
        blockedUser.Friends.RemoveAll(f => f.Id == user.Id);

        if (user.BlockedUsers.All(b => b.Id != blockedUser.Id))
        {
            user.BlockedUsers.Add(blockedUser);
        }
        
        await chatUsersRepository.SaveChangesAsync(cancellationToken);

        return ResultsHelper.NoContent();
    }
}

public class BlockUserRequestValidator : AbstractValidator<BlockUserRequest>
{
    public BlockUserRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.BlockedUserId)
            .NotEmpty().WithMessage("BlockedUserId is required.");

        RuleFor(x => x)
            .Must(x => x.UserId != x.BlockedUserId)
            .WithMessage("UserId and BlockedUserId must be different.");
    }
}

