using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;

namespace Application.Requests.Commands.Friends;

public record DenyPendingFriendRequest(Guid UserId, Guid FriendId) : IRequest;

public class DenyPendingFriendRequestHandler(IChatUsersRepository chatUsersRepository) : IRequestHandler<DenyPendingFriendRequest>
{
    public async Task<IOperationResult> HandleAsync(DenyPendingFriendRequest request, CancellationToken cancellationToken = default)
    {
        var user = await chatUsersRepository.GetByIdWithFriendsAsync(request.UserId, cancellationToken);
        if (user is null)
            return ResultsHelper.NotFound("User not found");

        var friendUser = await chatUsersRepository.GetByIdWithFriendsAsync(request.FriendId, cancellationToken);
        if (friendUser is null)
            return ResultsHelper.NotFound("Friend not found");

        var changed = false;
        if (user.WaitingFriendRequests.Contains(friendUser))
        {
            user.WaitingFriendRequests.Remove(friendUser);
            friendUser.FriendRequests.Remove(user);
            changed = true;
        }
        else if (user.FriendRequests.Contains(friendUser))
        {
            user.FriendRequests.Remove(friendUser);
            friendUser.WaitingFriendRequests.Remove(user);
            changed = true;
        }

        if (!changed)
            return ResultsHelper.BadRequest("Friend request not found");

        await chatUsersRepository.SaveChangesAsync(cancellationToken);
        return ResultsHelper.NoContent();
    }
}