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
            })
            .WithName("GetAllStickers");
        
        app.MapGet("/media/reactions/page={page:int}/size={size:int}",
            async ([FromRoute]ushort page, [FromRoute]ushort size, 
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var query = new GetAllSReactionsRequest(new PageSettings(page, size));
                var result = await sender.SendAsync(query, cancellationToken);
                
                return result.ToResult();
            })
            .WithName("GetAllReactions");
        
        app.MapPost("/media/stickers/",
            async ([FromForm] CreateStickerForm form, 
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var request = new CreateStickerRequest(form.StickerImage, form.Name);
                var result = await sender.SendAsync(request, cancellationToken);
                return result.ToResult();
            })
            .DisableAntiforgery()
            .WithName("CreateSticker");
        
        app.MapPost("/media/reactions/",
            async ([FromForm] CreateReactionForm form, 
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var request = new CreateReactionRequest(form.ReactionImage, form.Name, form.Description);
                var result = await sender.SendAsync(request, cancellationToken);
                return result.ToResult();
            })
            .DisableAntiforgery()
            .WithName("CreateReaction");
        
        app.MapDelete("/media/reactions/{id:guid}",
            async ([FromRoute] Guid id,
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var request = new DeleteReactionRequest(id);
                var result = await sender.SendAsync(request, cancellationToken);
                
                return result.ToResult();
            })
            .WithName("DeleteReaction");
        
        app.MapDelete("/media/stickers/{id:guid}",
            async ([FromRoute] Guid id,
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var request = new DeleteStickerRequest(id);
                var result = await sender.SendAsync(request, cancellationToken);
                
                return result.ToResult();
            })
            .WithName("DeleteSticker");
    }

    public class CreateStickerForm
    {
        public string Name { get; set; } = string.Empty;
        public IFormFile StickerImage { get; set; } = null!;
    }

    public class CreateReactionForm
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IFormFile ReactionImage { get; set; } = null!;
    }
}