using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Dtos.Requests.Shared;
using Application.Requests.Commands.Friends;
using Application.Requests.Queries.BlackList;
using Application.Requests.Queries.Friends;
using Microsoft.AspNetCore.Mvc;
using External.WebApi.Common.Helpers;
using WebApi.Common.Mappers;

namespace External.WebApi.Endpoints;

public static class Friends
{
    public static void MapFriendsEndpoints(this WebApplication app)
    {
        app.MapPost("/friends/",
            async ([FromBody] AddFriendRequest request, 
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);
                
                return result.ToResult();
            })
            .WithName("AddFriend");
        
        app.MapDelete("/friends/",
            async ([FromBody] DeleteFriendRequest request, 
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);
                
                return result.ToResult();
            })
            .WithName("DeleteFriend");
        
        app.MapGet("/friends/blacklist/page={page:int}/size={size:int}",
            async ([FromRoute]ushort page, [FromRoute]ushort size, [FromQuery] string searchQuery, 
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var query = new GetUserBlackListQuery(context.GetUserId(), searchQuery, new PageSettings(page, size));
                var result = await sender.SendAsync(query, cancellationToken);
                
                return result.ToResult();
            })
            .WithName("GetUserBlackList");
        
        app.MapPost("/friends/blacklist/",
            async ([FromBody] BlockUserRequest request, 
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);
                
                return result.ToResult();
            })
            .WithName("BlockUser");
        
        app.MapDelete("/friends/blacklist/",
            async ([FromBody] RemoveFromBlockList request,
                [FromServices]ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);
                
                return result.ToResult();
            })
            .WithName("RemoveFromBlockList");

        app.MapGet("/friends/", async (
            [FromQuery] Guid userId,
            [FromQuery] ushort pageNumber,
            [FromQuery] ushort pageSize,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetFriendsListQuery(userId, new PageSettings(pageNumber, pageSize));
            var result = await sender.SendAsync(query, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("GetFriendsList");
    }
}