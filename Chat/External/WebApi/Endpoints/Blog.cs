using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Requests.Commands.Posts;
using Application.Dtos.Requests.Shared;
using Application.Requests.Queries.Blog;
using Microsoft.AspNetCore.Mvc;
using WebApi.Common.Mappers;
using Microsoft.AspNetCore.Http;
using Application.Requests.Commands.Blog;
using External.WebApi.Common.Helpers;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Endpoints;

/// <summary>
/// Форма для добавления комментария
/// </summary>
public class AddCommentForm
{
    /// <summary>
    /// ID пользователя
    /// </summary>
    [SwaggerSchema(Description = "Уникальный идентификатор пользователя")]
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Содержимое комментария
    /// </summary>
    [SwaggerSchema(Description = "Текст комментария (от 1 до 1000 символов)")]
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Вложение к комментарию
    /// </summary>
    [SwaggerSchema(Description = "Файл вложения (опционально)")]
    public IFormFile? Attachment { get; set; }
    
    /// <summary>
    /// ID родительского комментария
    /// </summary>
    [SwaggerSchema(Description = "ID родительского комментария для ответа (опционально)")]
    public Guid? ParentCommentId { get; set; }
}

/// <summary>
/// Форма для ответа на комментарий
/// </summary>
public class ReplyToCommentForm
{
    /// <summary>
    /// ID пользователя
    /// </summary>
    [SwaggerSchema(Description = "Уникальный идентификатор пользователя")]
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Содержимое ответа
    /// </summary>
    [SwaggerSchema(Description = "Текст ответа (от 1 до 1000 символов)")]
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Вложение к ответу
    /// </summary>
    [SwaggerSchema(Description = "Файл вложения (опционально)")]
    public IFormFile? Attachment { get; set; }
}

public static class Blog
{
    /// <summary>
    /// Добавить новый пост
    /// </summary>
    [SwaggerOperation(
        Summary = "Создать новый пост",
        Description = "Создает новый пост с возможностью прикрепления файла",
        OperationId = "AddPost",
        Tags = new[] { "Blog" }
    )]
    [SwaggerResponse(201, "Пост успешно создан")]
    [SwaggerResponse(400, "Некорректные данные")]
    public static void MapBlogEndpoints(this WebApplication app)
    {
        app.MapPost("/blog/",
            async ([FromForm] AddPostRequest request,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);
                return result.ToResult();
            })
            .WithName("AddPost")
            .WithOpenApi()
            .WithTags("Blog")
            .Produces(201)
            .Produces(400);

        /// <summary>
        /// Удалить пост
        /// </summary>
        [SwaggerOperation(
            Summary = "Удалить пост",
            Description = "Удаляет пост по его ID",
            OperationId = "DeletePost",
            Tags = new[] { "Blog" }
        )]
        [SwaggerResponse(204, "Пост успешно удален")]
        [SwaggerResponse(404, "Пост не найден")]
        app.MapDelete("/blog/",
            async ([FromBody] DeletePostRequest request,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);
                return result.ToResult();
            })
            .WithName("DeletePost")
            .WithOpenApi()
            .WithTags("Blog")
            .Produces(204)
            .Produces(404);

        /// <summary>
        /// Добавить реакцию к посту
        /// </summary>
        [SwaggerOperation(
            Summary = "Добавить реакцию к посту",
            Description = "Добавляет реакцию (лайк, дизлайк и т.д.) к посту",
            OperationId = "SendReaction",
            Tags = new[] { "Blog" }
        )]
        [SwaggerResponse(201, "Реакция успешно добавлена")]
        [SwaggerResponse(400, "Некорректные данные")]
        app.MapPost("/blog/reactions/",
            async ([FromForm] SendReactionCommand request,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);
                return result.ToResult();
            })
            .WithName("SendReaction")
            .WithOpenApi()
            .WithTags("Blog")
            .Produces(201)
            .Produces(400);

        /// <summary>
        /// Переключить закрепление поста
        /// </summary>
        [SwaggerOperation(
            Summary = "Переключить закрепление поста",
            Description = "Закрепляет или открепляет пост",
            OperationId = "TogglePinPost",
            Tags = new[] { "Blog" }
        )]
        [SwaggerResponse(200, "Статус закрепления изменен")]
        [SwaggerResponse(404, "Пост не найден")]
        app.MapPatch("/blog/pin/toggle/",
            async ([FromBody] TogglePinPostRequest request,
                [FromServices] ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);
                return result.ToResult();
            })
            .WithName("TogglePinPost")
            .WithOpenApi()
            .WithTags("Blog")
            .Produces(200)
            .Produces(404);

        /// <summary>
        /// Получить посты пользователя
        /// </summary>
        [SwaggerOperation(
            Summary = "Получить посты пользователя",
            Description = "Возвращает список постов пользователя с пагинацией",
            OperationId = "GetUserPosts",
            Tags = new[] { "Blog" }
        )]
        [SwaggerResponse(200, "Список постов получен", typeof(List<Application.Dtos.Responses.Blog.PostListItemDto>))]
        [SwaggerResponse(404, "Пользователь не найден")]
        app.MapGet("/blog/user/{userId:guid}/posts", async (
            [FromRoute] Guid userId,
            [FromQuery] ushort pageNumber,
            [FromQuery] ushort pageSize,
            [FromQuery] bool desc,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetUserPostsQuery(userId, new PageSettings(pageNumber, pageSize), desc);
            var result = await sender.SendAsync(query, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("GetUserPosts")
        .WithOpenApi()
        .WithTags("Blog")
        .Produces<List<Application.Dtos.Responses.Blog.PostListItemDto>>(200)
        .Produces(404);

        /// <summary>
        /// Получить комментарии к посту
        /// </summary>
        [SwaggerOperation(
            Summary = "Получить комментарии к посту",
            Description = "Возвращает список комментариев к посту с пагинацией",
            OperationId = "GetPostComments",
            Tags = new[] { "Blog" }
        )]
        [SwaggerResponse(200, "Список комментариев получен", typeof(List<Application.Dtos.Responses.Blog.CommentDto>))]
        [SwaggerResponse(404, "Пост не найден")]
        app.MapGet("/blog/posts/{postId:guid}/comments", async (
            [FromRoute] Guid postId,
            [FromQuery] ushort pageNumber,
            [FromQuery] ushort pageSize,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetPostCommentsQuery(postId, new PageSettings(pageNumber, pageSize));
            var result = await sender.SendAsync(query, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("GetPostComments")
        .WithOpenApi()
        .WithTags("Blog")
        .Produces<List<Application.Dtos.Responses.Blog.CommentDto>>(200)
        .Produces(404);

        /// <summary>
        /// Добавить комментарий к посту
        /// </summary>
        [SwaggerOperation(
            Summary = "Добавить комментарий к посту",
            Description = "Добавляет новый комментарий к посту с возможностью вложения файла",
            OperationId = "AddComment",
            Tags = new[] { "Blog" }
        )]
        [SwaggerResponse(201, "Комментарий успешно добавлен")]
        [SwaggerResponse(400, "Некорректные данные")]
        [SwaggerResponse(404, "Пост не найден")]
        app.MapPost("/blog/posts/{postId:guid}/comments", async (
            [FromRoute] Guid postId,
            [FromForm] AddCommentForm form,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new AddCommentRequest(postId, form.UserId, form.Content, form.Attachment, form.ParentCommentId);
            var result = await sender.SendAsync(command, cancellationToken);
            return result.ToApiResult();
        })
        .DisableAntiforgery()
        .WithName("AddComment")
        .WithOpenApi()
        .WithTags("Blog")
        .Produces(201)
        .Produces(400)
        .Produces(404);

        /// <summary>
        /// Ответить на комментарий
        /// </summary>
        [SwaggerOperation(
            Summary = "Ответить на комментарий",
            Description = "Добавляет ответ на существующий комментарий",
            OperationId = "ReplyToComment",
            Tags = new[] { "Blog" }
        )]
        [SwaggerResponse(201, "Ответ успешно добавлен")]
        [SwaggerResponse(400, "Некорректные данные")]
        [SwaggerResponse(404, "Комментарий не найден")]
        app.MapPost("/blog/comments/{commentId:guid}/reply", async (
            [FromRoute] Guid commentId,
            [FromForm] ReplyToCommentForm form,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new ReplyToCommentRequest(commentId, form.UserId, form.Content, form.Attachment);
            var result = await sender.SendAsync(command, cancellationToken);
            return result.ToApiResult();
        })
        .DisableAntiforgery()
        .WithName("ReplyToComment")
        .WithOpenApi()
        .WithTags("Blog")
        .Produces(201)
        .Produces(400)
        .Produces(404);

        /// <summary>
        /// Переключить лайк поста
        /// </summary>
        [SwaggerOperation(
            Summary = "Переключить лайк поста",
            Description = "Добавляет или убирает лайк к посту",
            OperationId = "TogglePostLike",
            Tags = new[] { "Blog" }
        )]
        [SwaggerResponse(200, "Статус лайка изменен")]
        [SwaggerResponse(404, "Пост не найден")]
        app.MapPost("/blog/posts/{postId:guid}/toggle-like", async (
            [FromRoute] Guid postId,
            [FromBody] Guid userId,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new TogglePostLikeRequest(postId, userId);
            var result = await sender.SendAsync(command, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("TogglePostLike")
        .WithOpenApi()
        .WithTags("Blog")
        .Produces(200)
        .Produces(404);

        /// <summary>
        /// Переключить лайк комментария
        /// </summary>
        [SwaggerOperation(
            Summary = "Переключить лайк комментария",
            Description = "Добавляет или убирает лайк к комментарию",
            OperationId = "ToggleCommentLike",
            Tags = new[] { "Blog" }
        )]
        [SwaggerResponse(200, "Статус лайка изменен")]
        [SwaggerResponse(404, "Комментарий не найден")]
        app.MapPost("/blog/comments/{commentId:guid}/toggle-like", async (
            [FromRoute] Guid commentId,
            [FromBody] Guid userId,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new ToggleCommentLikeRequest(commentId, userId);
            var result = await sender.SendAsync(command, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("ToggleCommentLike")
        .WithOpenApi()
        .WithTags("Blog")
        .Produces(200)
        .Produces(404);

        /// <summary>
        /// Переключить комментарии к посту
        /// </summary>
        [SwaggerOperation(
            Summary = "Переключить комментарии к посту",
            Description = "Включает или отключает возможность комментирования поста",
            OperationId = "TogglePostComments",
            Tags = new[] { "Blog" }
        )]
        [SwaggerResponse(200, "Статус комментариев изменен")]
        [SwaggerResponse(404, "Пост не найден")]
        app.MapPatch("/blog/posts/{postId:guid}/toggle-comments", async (
            [FromRoute] Guid postId,
            [FromBody] Guid userId,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new TogglePostCommentsRequest(postId, userId);
            var result = await sender.SendAsync(command, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("TogglePostComments")
        .WithOpenApi()
        .WithTags("Blog")
        .Produces(200)
        .Produces(404);
    }
}