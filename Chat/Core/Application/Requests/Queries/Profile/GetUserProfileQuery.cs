using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Common.Mappers;
using Application.Dtos.Responses.Profile;
using Domain.Models.Users;

namespace Application.Requests.Queries.Profile;

public record GetUserProfileQuery(Guid RequestedUserId, Guid CurrentUserId) : IRequest;

public class GetUserProfileQueryHandler(IChatUsersRepository chatUsersRepository) : IRequestHandler<GetUserProfileQuery>
{
    public async Task<IOperationResult> HandleAsync(GetUserProfileQuery request, CancellationToken cancellationToken = default)
    {
        var user = await chatUsersRepository.GetByIdAsync(request.RequestedUserId, cancellationToken);
        
        if (user is null)
        {
            return ResultsHelper.NotFound("User not found");
        }

        var canViewFullProfile = request.RequestedUserId == request.CurrentUserId || 
                                user.PrivacySetting.Id == (int)PrivacySettingsEnums.Public ||
                                user.PrivacySetting.Id == (int)PrivacySettingsEnums.Friends;

        var profileDto = user.ToProfileDto(canViewFullProfile);
        
        return ResultsHelper.Ok(profileDto);
    }
} 