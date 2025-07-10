using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Common.Mappers;
using Application.Services.ApplicationInfrastructure.Results;

namespace Application.Requests.Queries.Profile;

public record GetOwnProfileQuery(Guid CurrentUserId) : IRequest;

public class GetOwnProfileQueryHandler(IChatUsersRepository chatUsersRepository, IFilesSigningService filesSigningService) : IRequestHandler<GetOwnProfileQuery>
{
    public async Task<IOperationResult> HandleAsync(GetOwnProfileQuery request, CancellationToken cancellationToken = default)
    {
        var user = await chatUsersRepository.GetByIdWithFriendsAsNoTrackingAsync(request.CurrentUserId, cancellationToken);
        if (user is null)
        {
            return ResultsHelper.NotFound("User not found");
        }

        var dto = await user.ToOwnProfileDtoAsync(filesSigningService, cancellationToken);

        return ResultsHelper.Ok(dto);
    }
} 