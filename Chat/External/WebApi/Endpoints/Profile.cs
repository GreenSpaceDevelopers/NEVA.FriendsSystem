using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Requests.Commands.Profile;
using Application.Requests.Queries.Profile;
using Microsoft.AspNetCore.Mvc;
using External.WebApi.Common.Helpers;
using WebApi.Common.Mappers;

namespace WebApi.Endpoints;

public static class Profile
{
    public static void MapProfileEndpoints(this WebApplication app)
    {
        app.MapGet("/profile/{requestedUserId:guid}",
            async ([FromRoute] Guid requestedUserId, [FromQuery] Guid currentUserId,
                [FromServices] ISender sender, CancellationToken cancellationToken) =>
            {
                var query = new GetUserProfileQuery(requestedUserId, currentUserId);
                var result = await sender.SendAsync(query, cancellationToken);
                
                return result.ToResult();
            });

        app.MapPut("/profile/",
            async ([FromForm] UpdateProfileRequest request,
                [FromServices] ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);
                
                return result.ToResult();
            });

        app.MapPost("/profile/validate-username/",
            async ([FromBody] ValidateUsernameRequest request,
                [FromServices] ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);
                
                return result.ToResult();
            });
    }
} 