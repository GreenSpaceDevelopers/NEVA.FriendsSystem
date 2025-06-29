using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Dtos.Requests.Shared;
using Application.Dtos.Responses.BlackList;
using Application.Dtos.Responses.Friends;
using Application.Requests.Commands.Friends;
using Application.Requests.Queries.BlackList;
using Application.Requests.Queries.Friends;
using WebApi.Common.Helpers;
using Microsoft.AspNetCore.Mvc;
using WebApi.Common.Mappers;
using HttpContext = Microsoft.AspNetCore.Http.HttpContext;

namespace WebApi.Endpoints;

public static class Friends
{
    public static void MapFriendsEndpoints(this WebApplication app)
    {
        app.MapPost("/friends/",
            async ([FromBody] AddFriendRequest request,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);

                return result.ToResult();
            })
            .WithName("AddFriend")
            .WithOpenApi()
            .WithTags("Friends")
            .Produces(201)
            .Produces(400)
            .Produces(404);

        app.MapDelete("/friends/",
            async ([FromBody] DeleteFriendRequest request,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);

                return result.ToResult();
            })
            .WithName("DeleteFriend")
            .WithOpenApi()
            .WithTags("Friends")
            .Produces(204)
            .Produces(404);

        app.MapGet("/friends/blacklist/page={page:int}/size={size:int}",
            async ([FromRoute] ushort page, [FromRoute] ushort size, [FromQuery] string searchQuery,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var query = new GetUserBlackListQuery(context.GetUserId(), searchQuery, new PageSettings(page, size));
                var result = await sender.SendAsync(query, cancellationToken);

                return result.ToResult();
            })
            .WithName("GetUserBlackList")
            .WithOpenApi()
            .WithTags("Friends")
            .Produces<List<BlackListItemDto>>(200);

        app.MapPost("/friends/accept/",
            async ([FromBody] AcceptPendingFriendRequest request,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);

                return result.ToResult();
            })
            .WithName("AcceptFriendRequest")
            .WithOpenApi()
            .WithTags("Friends")
            .Produces(200)
            .Produces(404);

        app.MapPost("/friends/deny/",
            async ([FromBody] DenyPendingFriendRequest request,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);

                return result.ToResult();
            })
            .WithName("DenyFriendRequest")
            .WithOpenApi()
            .WithTags("Friends")
            .Produces(200)
            .Produces(404);

        app.MapPost("/friends/blacklist/",
            async ([FromBody] BlockUserRequest request,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);

                return result.ToResult();
            })
            .WithName("BlockUser")
            .WithOpenApi()
            .WithTags("Friends")
            .Produces(200)
            .Produces(400)
            .Produces(404);

        app.MapDelete("/friends/blacklist/",
            async ([FromBody] RemoveFromBlockList request,
                [FromServices] ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);

                return result.ToResult();
            })
            .WithName("RemoveFromBlockList")
            .WithOpenApi()
            .WithTags("Friends")
            .Produces(200)
            .Produces(404);

        app.MapGet("/friends/", async (
            [FromQuery] Guid userId,
            [FromQuery] int skip,
            [FromQuery] int take,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetFriendsListQuery(userId, new PageSettings(skip, take));
            var result = await sender.SendAsync(query, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("GetFriendsList")
        .WithOpenApi()
        .WithTags("Friends")
        .Produces<List<FriendDto>>(200)
        .Produces(404);

        app.MapGet("/friends/blacklist/page={page:int}/size={size:int}",
                async ([FromRoute] ushort page, [FromRoute] ushort size,
                    [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
                {
                    var query = new GetUserSentRequests(context.GetUserId(), new PageSettings(page, size));
                    var result = await sender.SendAsync(query, cancellationToken);

                    return result.ToResult();
                })
            .WithName("GetUsersSendedRequests")
            .WithOpenApi()
            .WithTags("Friends")
            .Produces<List<FriendRequestsDto>>(200);
        
        app.MapGet("/friends/requests/page={page:int}/size={size:int}",
                async ([FromRoute] ushort page, [FromRoute] ushort size,
                    [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
                {
                    var query = new GetUserPendingRequests(context.GetUserId(), new PageSettings(page, size));
                    var result = await sender.SendAsync(query, cancellationToken);

                    return result.ToResult();
                })
            .WithName("GetUsersPendingRequests")
            .WithOpenApi()
            .WithTags("Friends")
            .Produces<List<FriendRequestsDto>>(200);
    }
}