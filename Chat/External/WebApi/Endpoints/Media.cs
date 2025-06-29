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
    /// <summary>
    /// Получить стикеры
    /// </summary>
    [SwaggerOperation(
        Summary = "Получить стикеры",
        Description = "Возвращает список стикеров с пагинацией",
        OperationId = "GetAllStickers",
        Tags = new[] { "Media" }
    )]
    [SwaggerResponse(200, "Список стикеров получен", typeof(List<Application.Dtos.MediaDto>))]
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
            .WithName("GetAllStickers")
            .WithOpenApi()
            .WithTags("Media")
            .Produces<List<Application.Dtos.MediaDto>>(200);
        
        /// <summary>
        /// Получить реакции
        /// </summary>
        [SwaggerOperation(
            Summary = "Получить реакции",
            Description = "Возвращает список реакций с пагинацией",
            OperationId = "GetAllReactions",
            Tags = new[] { "Media" }
        )]
        [SwaggerResponse(200, "Список реакций получен", typeof(List<Application.Dtos.MediaDto>))]
        app.MapGet("/media/reactions/page={page:int}/size={size:int}",
            async ([FromRoute]ushort page, [FromRoute]ushort size, 
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var query = new GetAllSReactionsRequest(new PageSettings(page, size));
                var result = await sender.SendAsync(query, cancellationToken);
                
                return result.ToResult();
            })
            .WithName("GetAllReactions")
            .WithOpenApi()
            .WithTags("Media")
            .Produces<List<Application.Dtos.MediaDto>>(200);
        
        /// <summary>
        /// Создать стикер
        /// </summary>
        [SwaggerOperation(
            Summary = "Создать стикер",
            Description = "Создает новый стикер с загруженным изображением",
            OperationId = "CreateSticker",
            Tags = new[] { "Media" }
        )]
        [SwaggerResponse(201, "Стикер успешно создан")]
        [SwaggerResponse(400, "Некорректные данные")]
        app.MapPost("/media/stickers/",
            async ([FromForm] CreateStickerForm form, 
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
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
        
        /// <summary>
        /// Создать реакцию
        /// </summary>
        [SwaggerOperation(
            Summary = "Создать реакцию",
            Description = "Создает новую реакцию с загруженным изображением",
            OperationId = "CreateReaction",
            Tags = new[] { "Media" }
        )]
        [SwaggerResponse(201, "Реакция успешно создана")]
        [SwaggerResponse(400, "Некорректные данные")]
        app.MapPost("/media/reactions/",
            async ([FromForm] CreateReactionForm form, 
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
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
        
        /// <summary>
        /// Удалить реакцию
        /// </summary>
        [SwaggerOperation(
            Summary = "Удалить реакцию",
            Description = "Удаляет реакцию по её ID",
            OperationId = "DeleteReaction",
            Tags = new[] { "Media" }
        )]
        [SwaggerResponse(204, "Реакция успешно удалена")]
        [SwaggerResponse(404, "Реакция не найдена")]
        app.MapDelete("/media/reactions/{id:guid}",
            async ([FromRoute] Guid id,
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
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
        
        /// <summary>
        /// Удалить стикер
        /// </summary>
        [SwaggerOperation(
            Summary = "Удалить стикер",
            Description = "Удаляет стикер по его ID",
            OperationId = "DeleteSticker",
            Tags = new[] { "Media" }
        )]
        [SwaggerResponse(204, "Стикер успешно удален")]
        [SwaggerResponse(404, "Стикер не найден")]
        app.MapDelete("/media/stickers/{id:guid}",
            async ([FromRoute] Guid id,
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
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