using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;

namespace Application.Requests.Commands.Friends;

public record AddFriendRequest(Guid UserId, Guid FriendId) : IRequest;

public class AddFriendRequestHandler(IChatUsersRepository chatUsersRepository) : IRequestHandler<AddFriendRequest>
{
    public async Task<IOperationResult> HandleAsync(AddFriendRequest request, CancellationToken cancellationToken = default)
    {
        var user = await chatUsersRepository.GetByIdWithFriendsAsync(request.UserId, cancellationToken);
        
        if (user is null)
        {
            return ResultsHelper.NotFound("User not found");
        }
        
        var friend = await chatUsersRepository.GetByIdWithFriendsAsync(request.FriendId, cancellationToken);
        
        if (friend is null)
        {
            return ResultsHelper.NotFound("Friend not found");
        }

        if (user.Friends.Contains(friend))
        {
            return ResultsHelper.BadRequest("Friend already added");
        }
        
        if (friend.BlockedUsers.Contains(user))
        {
            return ResultsHelper.BadRequest("User is blocked");
        }
        
        if (user.WaitingFriendRequests.Contains(friend))
        {
            return ResultsHelper.BadRequest("Friend request already sent");
        }
        
        user.WaitingFriendRequests.Add(friend);
        friend.FriendRequests.Add(user);
        await chatUsersRepository.SaveChangesAsync(cancellationToken);
        
        return ResultsHelper.NoContent();
    }
}