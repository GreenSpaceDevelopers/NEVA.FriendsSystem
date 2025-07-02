using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Dtos.Responses.Profile;
using Application.Services.ApplicationInfrastructure.Results;

namespace Application.Requests.Queries.Privacy;

public record GetUserPrivacySettingsQuery(Guid UserId) : IRequest;

public class GetUserPrivacySettingsQueryHandler(IPrivacyRepository privacyRepository) : IRequestHandler<GetUserPrivacySettingsQuery>
{
    public async Task<IOperationResult> HandleAsync(GetUserPrivacySettingsQuery request, CancellationToken cancellationToken = default)
    {
        var settings = await privacyRepository.GetUserPrivacySettingsAsync(request.UserId, cancellationToken);
        
        if (settings is null)
        {
            settings = await privacyRepository.CreateDefaultPrivacySettingsAsync(request.UserId, cancellationToken);
            await privacyRepository.SaveChangesAsync(cancellationToken);
        }

        var dto = new UserPrivacySettingsDto(
            settings.Id,
            settings.FriendsListVisibility,
            settings.CommentsPermission,
            settings.DirectMessagesPermission
        );

        return ResultsHelper.Ok(dto);
    }
} 