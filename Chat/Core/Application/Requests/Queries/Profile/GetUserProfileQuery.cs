using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Common.Mappers;
using Application.Services.ApplicationInfrastructure.Results;
using Domain.Models.Users;
using Microsoft.Extensions.Logging;

namespace Application.Requests.Queries.Profile;

public record GetUserProfileQuery(Guid RequestedUserId, Guid CurrentUserId) : IRequest;

public class GetUserProfileQueryHandler(IChatUsersRepository chatUsersRepository, ILogger<GetUserProfileQueryHandler> logger) : IRequestHandler<GetUserProfileQuery>
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

        var profileDto = user.ToProfileDto(canViewFullProfile);

        return ResultsHelper.Ok(profileDto);
    }
}