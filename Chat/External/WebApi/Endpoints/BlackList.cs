using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Dtos.Requests.Shared;
using Application.Requests.Commands.Friends;
using Application.Requests.Queries.BlackList;
using Microsoft.AspNetCore.Mvc;
using External.WebApi.Common.Helpers;

namespace WebApi.Endpoints;

public static class BlackList
{
    public static void MapBlackListEndpoints(this WebApplication app)
    {
        app.MapGet("/users/search", async (
            [FromQuery] Guid currentUserId,
            [FromQuery] string query,
            [FromQuery] ushort pageNumber,
            [FromQuery] ushort pageSize,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var searchQuery = new SearchUsersQuery(currentUserId, query, new PageSettings(pageNumber, pageSize));
            var result = await sender.SendAsync(searchQuery, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("SearchUsers");
    }
} 