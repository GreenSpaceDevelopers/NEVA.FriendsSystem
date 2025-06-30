using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Dtos.Requests.Shared;
using Application.Requests.Queries.BlackList;
using Microsoft.AspNetCore.Mvc;
using WebApi.Common.Helpers;

namespace WebApi.Endpoints;

public static class BlackList
{
    public static void MapBlackListEndpoints(this WebApplication app)
    {
        app.MapGet("/users/search", async (
            [FromQuery] string query,
            [FromQuery] int skip,
            [FromQuery] int take,
            [FromServices] ISender sender, HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var searchQuery = new SearchUsersQuery(context.GetUserId(), query, new PageSettings(skip, take));
            var result = await sender.SendAsync(searchQuery, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("SearchUsers")
        .WithOpenApi()
        .WithTags("BlackList")
        .Produces<List<Application.Dtos.Responses.BlackList.UserSearchDto>>(200)
        .Produces(404);
    }
}