using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using FluentValidation;
using Application.Abstractions.Services.Notifications;
using NEVA.BackendApi.Constants;

namespace Application.Requests.Commands.Friends;

public record AcceptPendingFriendRequest(Guid UserId, Guid FriendId) : IRequest;

public class AcceptPendingFriendRequestHandler(IChatUsersRepository chatUsersRepository, IBackendNotificationService notificationService) : IRequestHandler<AcceptPendingFriendRequest>
{
    public async Task<IOperationResult> HandleAsync(AcceptPendingFriendRequest request, CancellationToken cancellationToken = default)
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

        if (!user.FriendRequests.Contains(friendUser))
        {
            return ResultsHelper.BadRequest("Friend request not found");
        }

        user.Friends.Add(friendUser);
        friendUser.Friends.Add(user);

        user.FriendRequests.Remove(friendUser);
        friendUser.WaitingFriendRequests.Remove(user);

        await chatUsersRepository.SaveChangesAsync(cancellationToken);

        // Notify original requester that their friend request was accepted
        var receiverParams = new List<string> { "#", user.Username ?? user.AspNetUser.UserName };
        await notificationService.SendNotificationAsync(
            NotificationTemplatesConsts.FriendAccepted.Id,
            request.FriendId,
            request.UserId,
            false,
            receiverParams,
            null);

        return ResultsHelper.NoContent();
    }
}

public class AcceptPendingFriendRequestValidator : AbstractValidator<AcceptPendingFriendRequest>
{
    public AcceptPendingFriendRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.FriendId)
            .NotEmpty().WithMessage("FriendId is required.");

        RuleFor(x => x)
            .Must(x => x.UserId != x.FriendId)
            .WithMessage("UserId and FriendId must be different.");
    }
}