using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Dtos.Requests.Shared;
using Application.Requests.Commands.Friends;
using Application.Requests.Queries.BlackList;
using Microsoft.AspNetCore.Mvc;
using External.WebApi.Common.Helpers;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Endpoints;

public static class BlackList
{
    /// <summary>
    /// Поиск пользователей
    /// </summary>
    [SwaggerOperation(
        Summary = "Поиск пользователей",
        Description = "Выполняет поиск пользователей по имени с исключением заблокированных",
        OperationId = "SearchUsers",
        Tags = new[] { "BlackList" }
    )]
    [SwaggerResponse(200, "Результаты поиска получены", typeof(List<Application.Dtos.Responses.BlackList.UserSearchDto>))]
    [SwaggerResponse(404, "Пользователь не найден")]
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
        .WithName("SearchUsers")
        .WithOpenApi()
        .WithTags("BlackList")
        .Produces<List<Application.Dtos.Responses.BlackList.UserSearchDto>>(200)
        .Produces(404);
    }
} 