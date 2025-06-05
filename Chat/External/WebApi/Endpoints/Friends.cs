using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Requests.Commands;
using Application.Requests.Commands.Friends;
using Microsoft.AspNetCore.Mvc;
using WebApi.Common.Mappers;

namespace WebApi.Endpoints;

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
            });
        
        app.MapDelete("/friends/",
            async ([FromBody] DeleteFriendRequest request, 
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);
                
                return result.ToResult();
            });
        
        app.MapPost("/friends/blacklist/",
            async ([FromBody] BlockUserRequest request, 
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);
                
                return result.ToResult();
            });
        
        // app.MapPut("/friends/blacklist/",
        //     async ([FromBody] RemoveFromBlockList
        //         [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
        //     {
        //         var request = new DeleteStickerRequest(id);
        //         var result = await sender.SendAsync(request, cancellationToken);
        //         
        //         return result.ToResult();
        //     });
    }
}