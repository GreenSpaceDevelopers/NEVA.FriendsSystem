using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Common.Models;
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
            async ([FromBody] AddFriendApiRequest request,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var command = new AddFriendRequest(context.GetUserId(), request.FriendId);
                var result = await sender.SendAsync(command, cancellationToken);
                return result.ToResult();
            })
            .WithName("AddFriend")
            .WithOpenApi()
            .WithTags("Friends")
            .Produces(201)
            .Produces(400)
            .Produces(404);

        app.MapDelete("/friends/{friendId:guid}",
            async ([FromRoute] Guid friendId,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var command = new DeleteFriendRequest(context.GetUserId(), friendId);
                var result = await sender.SendAsync(command, cancellationToken);
                return result.ToResult();
            })
            .WithName("DeleteFriend")
            .WithOpenApi()
            .WithTags("Friends")
            .Produces(204)
            .Produces(404);

        app.MapGet("/friends/blacklist",
            async ([FromQuery] ushort skip, [FromQuery] ushort take, [FromQuery] string? searchQuery,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var query = new GetUserBlackListQuery(context.GetUserId(), searchQuery, new PageSettings(skip, take));
                var result = await sender.SendAsync(query, cancellationToken);

                return result.ToResult();
            })
            .WithName("GetUserBlackList")
            .WithOpenApi()
            .WithTags("Friends")
            .Produces<PagedList<BlackListItemDto>>(200);

        app.MapPost("/friends/accept/",
            async ([FromBody] AcceptPendingFriendApiRequest request,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var command = new AcceptPendingFriendRequest(context.GetUserId(), request.FriendId);
                var result = await sender.SendAsync(command, cancellationToken);
                return result.ToResult();
            })
            .WithName("AcceptFriendRequest")
            .WithOpenApi()
            .WithTags("Friends")
            .Produces(200)
            .Produces(404);

        app.MapPost("/friends/deny/",
            async ([FromBody] DenyPendingFriendApiRequest request,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var command = new DenyPendingFriendRequest(context.GetUserId(), request.FriendId);
                var result = await sender.SendAsync(command, cancellationToken);
                return result.ToResult();
            })
            .WithName("DenyFriendRequest")
            .WithOpenApi()
            .WithTags("Friends")
            .Produces(200)
            .Produces(404);

        app.MapPost("/friends/blacklist/",
            async ([FromBody] BlockUserApiRequest request,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var command = new BlockUserRequest(context.GetUserId(), request.BlockedUserId);
                var result = await sender.SendAsync(command, cancellationToken);
                return result.ToResult();
            })
            .WithName("BlockUser")
            .WithOpenApi()
            .WithTags("Friends")
            .Produces(200)
            .Produces(400)
            .Produces(404);

        app.MapDelete("/friends/blacklist/{unblockedUserId:guid}",
            async ([FromRoute] Guid unblockedUserId,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var command = new RemoveFromBlockList(context.GetUserId(), unblockedUserId);
                var result = await sender.SendAsync(command, cancellationToken);
                return result.ToResult();
            })
            .WithName("RemoveFromBlockList")
            .WithOpenApi()
            .WithTags("Friends")
            .Produces(200)
            .Produces(404);

        app.MapGet("/friends/", async (
            [FromQuery] int skip,
            [FromQuery] int take,
            [FromQuery] Guid? disciplineId,
            [FromQuery] string? searchQuery,
            [FromServices] ISender sender, HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var query = new GetFriendsListQuery(context.GetUserId(), new PageSettings(skip, take), disciplineId, searchQuery);
            var result = await sender.SendAsync(query, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("GetFriendsList")
        .WithOpenApi()
        .WithTags("Friends")
        .Produces<PagedList<FriendDto>>(200)
        .Produces(404);

        app.MapGet("/friends/requests/sent",
                async ([FromQuery] ushort skip, [FromQuery] ushort take,
                    [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
                {
                    var query = new GetUserSentRequests(context.GetUserId(), new PageSettings(skip, take));
                    var result = await sender.SendAsync(query, cancellationToken);

                    return result.ToResult();
                })
            .WithName("GetUsersSendedRequests")
            .WithOpenApi()
            .WithTags("Friends")
            .Produces<PagedList<FriendRequestsDto>>(200);
        
        app.MapGet("/friends/requests/pending",
                async ([FromQuery] ushort skip, [FromQuery] ushort take,
                    [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
                {
                    var query = new GetUserPendingRequests(context.GetUserId(), new PageSettings(skip, take));
                    var result = await sender.SendAsync(query, cancellationToken);

                    return result.ToResult();
                })
            .WithName("GetUsersPendingRequests")
            .WithOpenApi()
            .WithTags("Friends")
            .Produces<PagedList<FriendRequestsDto>>(200);
    }

    private record BlockUserApiRequest(Guid BlockedUserId);

    private record AcceptPendingFriendApiRequest(Guid FriendId);

    private record DenyPendingFriendApiRequest(Guid FriendId);

    private record AddFriendApiRequest(Guid FriendId);
}