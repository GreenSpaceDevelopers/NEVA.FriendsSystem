using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Dtos.Requests.Shared;
using Application.Requests.Commands;
using Application.Requests.Queries.Media;
using Microsoft.AspNetCore.Mvc;
using WebApi.Common.Mappers;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Endpoints;

/// <summary>
/// Форма для создания стикера
/// </summary>
public class CreateStickerForm
{
    /// <summary>
    /// Изображение стикера
    /// </summary>
    [SwaggerSchema(Description = "Файл изображения стикера")]
    public IFormFile StickerImage { get; set; } = null!;

    /// <summary>
    /// Название стикера
    /// </summary>
    [SwaggerSchema(Description = "Название стикера")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Форма для создания реакции
/// </summary>
public class CreateReactionForm
{
    /// <summary>
    /// Изображение реакции
    /// </summary>
    [SwaggerSchema(Description = "Файл изображения реакции")]
    public IFormFile ReactionImage { get; set; } = null!;

    /// <summary>
    /// Название реакции
    /// </summary>
    [SwaggerSchema(Description = "Название реакции")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание реакции
    /// </summary>
    [SwaggerSchema(Description = "Описание реакции")]
    public string Description { get; set; } = string.Empty;
}

public static class Media
{
    public static void MapMediaEndpoints(this WebApplication app)
    {
        app.MapGet("/media/stickers",
            async ([FromQuery] ushort skip, [FromQuery] ushort take,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var query = new GetAllStickersRequest(new PageSettings(skip, take));
                var result = await sender.SendAsync(query, cancellationToken);

                return result.ToResult();
            })
            .WithName("GetAllStickers")
            .WithOpenApi()
            .WithTags("Media")
            .Produces<List<Application.Dtos.MediaDto>>(200);

        app.MapGet("/media/reactions",
            async ([FromQuery] ushort skip, [FromQuery] ushort take,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var query = new GetAllSReactionsRequest(new PageSettings(skip, take));
                var result = await sender.SendAsync(query, cancellationToken);

                return result.ToResult();
            })
            .WithName("GetAllReactions")
            .WithOpenApi()
            .WithTags("Media")
            .Produces<List<Application.Dtos.MediaDto>>(200);

        app.MapPost("/media/stickers/",
            async ([FromForm] CreateStickerForm form,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var request = new CreateStickerRequest(form.StickerImage, form.Name);
                var result = await sender.SendAsync(request, cancellationToken);
                return result.ToResult();
            })
            .DisableAntiforgery()
            .WithName("CreateSticker")
            .WithOpenApi()
            .WithTags("Media")
            .Produces(201)
            .Produces(400);

        app.MapPost("/media/reactions/",
            async ([FromForm] CreateReactionForm form,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var request = new CreateReactionRequest(form.ReactionImage, form.Name, form.Description);
                var result = await sender.SendAsync(request, cancellationToken);
                return result.ToResult();
            })
            .DisableAntiforgery()
            .WithName("CreateReaction")
            .WithOpenApi()
            .WithTags("Media")
            .Produces(201)
            .Produces(400);

        app.MapDelete("/media/reactions/{id:guid}",
            async ([FromRoute] Guid id,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var request = new DeleteReactionRequest(id);
                var result = await sender.SendAsync(request, cancellationToken);

                return result.ToResult();
            })
            .WithName("DeleteReaction")
            .WithOpenApi()
            .WithTags("Media")
            .Produces(204)
            .Produces(404);

        app.MapDelete("/media/stickers/{id:guid}",
            async ([FromRoute] Guid id,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var request = new DeleteStickerRequest(id);
                var result = await sender.SendAsync(request, cancellationToken);

                return result.ToResult();
            })
            .WithName("DeleteSticker")
            .WithOpenApi()
            .WithTags("Media")
            .Produces(204)
            .Produces(404);
    }
}