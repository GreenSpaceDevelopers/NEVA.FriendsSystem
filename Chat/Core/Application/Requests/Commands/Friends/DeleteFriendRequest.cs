using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;

namespace Application.Requests.Commands.Friends;

public record DeleteFriendRequest(Guid UserId, Guid FriendId) : IRequest;

public class DeleteFriendRequestHandler(IChatUsersRepository chatUsersRepository) : IRequestHandler<DeleteFriendRequest>
{
    public async Task<IOperationResult> HandleAsync(DeleteFriendRequest request, CancellationToken cancellationToken = default)
    {
        var user = await chatUsersRepository.GetByIdWithFriendsAsync(request.UserId, cancellationToken);
        
        if (user is null)
        {
            return ResultsHelper.NotFound("User not found");
        }
        
        var friendUser = await chatUsersRepository.GetByIdWithFriendsAsync(request.FriendId, cancellationToken);
        
        if (friendUser is null)
        {
            return ResultsHelper.NotFound("Friend not found");
        }
        
        if (!user.Friends.Contains(friendUser))
        {
            return ResultsHelper.BadRequest("User is not a friend");
        }
        
        user.Friends.Remove(friendUser);
        friendUser.Friends.Remove(user);
        
        await chatUsersRepository.SaveChangesAsync(cancellationToken);
        
        return ResultsHelper.NoContent();
    }
}

