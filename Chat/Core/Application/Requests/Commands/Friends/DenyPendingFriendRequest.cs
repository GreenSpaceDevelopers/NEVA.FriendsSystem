using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;

namespace Application.Requests.Commands.Friends;

public record DenyPendingFriendRequest(Guid UserId, Guid FriendId) : IRequest;

public class DenyPendingFriendRequestHandler(IChatUsersRepository chatUsersRepository) : IRequestHandler<DenyPendingFriendRequest>
{
    public async Task<IOperationResult> HandleAsync(DenyPendingFriendRequest request, CancellationToken cancellationToken = default)
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
        
        if (!user.WaitingFriendRequests.Contains(friendUser))
        {
            return ResultsHelper.BadRequest("Friend request not found");
        }
        
        user.FriendRequests.Remove(friendUser);
        await chatUsersRepository.SaveChangesAsync(cancellationToken);
        
        return ResultsHelper.NoContent();
    }
}