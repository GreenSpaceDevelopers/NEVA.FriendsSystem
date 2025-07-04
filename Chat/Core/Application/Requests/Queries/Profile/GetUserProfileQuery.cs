using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Common.Mappers;
using Application.Services.ApplicationInfrastructure.Results;
using Domain.Models.Users;

namespace Application.Requests.Queries.Profile;

public record GetUserProfileQuery(Guid RequestedUserId, Guid CurrentUserId) : IRequest;

public class GetUserProfileQueryHandler(IChatUsersRepository chatUsersRepository) : IRequestHandler<GetUserProfileQuery>
{
    public async Task<IOperationResult> HandleAsync(GetUserProfileQuery request, CancellationToken cancellationToken = default)
    {
        var user = await chatUsersRepository.GetByIdWithProfileDataAsync(request.RequestedUserId, cancellationToken);
        if (user is null)
        {
            return ResultsHelper.NotFound($"User not found. RequestedUserId: {request.RequestedUserId}");
        }

        var canViewFullProfile = request.RequestedUserId == request.CurrentUserId ||
                                user.PrivacySettings.FriendsListVisibility == PrivacyLevel.Public;

        var isBlockedByMe = await chatUsersRepository.IsUserBlockedByAsync(request.RequestedUserId, request.CurrentUserId, cancellationToken);
        var hasBlockedMe = await chatUsersRepository.IsUserBlockedByAsync(request.CurrentUserId, request.RequestedUserId, cancellationToken);

        if (hasBlockedMe)
        {
            canViewFullProfile = false;
        }

        var profileDto = user.ToProfileDto(canViewFullProfile, isBlockedByMe, hasBlockedMe);

        return ResultsHelper.Ok(profileDto);
    }
}