using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Dtos.Requests.Shared;
using Application.Requests.Commands;
using Application.Requests.Queries.Media;
using Microsoft.AspNetCore.Mvc;
using WebApi.Common.Mappers;

namespace WebApi.Endpoints;

public static class Media
{
    public static void MapMediaEndpoints(this WebApplication app)
    {
        app.MapGet("/media/stickers/page={page:int}/size={size:int}",
            async ([FromRoute]ushort page, [FromRoute]ushort size, 
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var query = new GetAllStickersRequest(new PageSettings(page, size));
                var result = await sender.SendAsync(query, cancellationToken);
                
                return result.ToResult();
            });
        
        app.MapGet("/media/reactions/page={page:int}/size={size:int}",
            async ([FromRoute]ushort page, [FromRoute]ushort size, 
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var query = new GetAllStickersRequest(new PageSettings(page, size));
                var result = await sender.SendAsync(query, cancellationToken);
                
                return result.ToResult();
            });
        
        app.MapPost("/media/stickers/",
            async ([FromForm] CreateStickerRequest request, 
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);
                
                return result.ToResult();
            });
        
        app.MapPost("/media/reactions/",
            async ([FromForm] CreateReactionRequest request, 
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);
                
                return result.ToResult();
            });
        
        app.MapDelete("/media/reactions/{id:guid}",
            async ([FromRoute] Guid id,
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var request = new DeleteReactionRequest(id);
                var result = await sender.SendAsync(request, cancellationToken);
                
                return result.ToResult();
            });
        
        app.MapDelete("/media/stickers/{id:guid}",
            async ([FromRoute] Guid id,
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var request = new DeleteStickerRequest(id);
                var result = await sender.SendAsync(request, cancellationToken);
                
                return result.ToResult();
            });
    }
}