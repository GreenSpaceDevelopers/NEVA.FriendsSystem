using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Common.Models;
using Application.Requests.Commands.Posts;
using Application.Dtos.Requests.Shared;
using Application.Dtos.Responses.Blog;
using Application.Requests.Queries.Blog;
using Microsoft.AspNetCore.Mvc;
using WebApi.Common.Mappers;
using Application.Requests.Commands.Blog;
using WebApi.Common.Helpers;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Endpoints;

/// <summary>
/// Форма для добавления комментария
/// </summary>
public class AddCommentForm
{
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
    public static void MapBlogEndpoints(this WebApplication app)
    {
        app.MapPost("/blog/",
            async ([FromForm] AddPostApiRequest request,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var command = new AddPostRequest
                {
                    File = request.File,
                    UserId = context.GetUserId(),
                    Content = request.Content,
                    Title = request.Title
                };
                var result = await sender.SendAsync(command, cancellationToken);
                return result.ToResult();
            })
            .DisableAntiforgery()
            .WithName("AddPost")
            .WithOpenApi()
            .WithTags("Blog")
            .Produces(201)
            .Produces(400);

        app.MapDelete("/blog/{id:guid}",
            async ([FromRoute] Guid id,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var command = new DeletePostRequest(id, context.GetUserId());
                var result = await sender.SendAsync(command, cancellationToken);
                return result.ToResult();
            })
            .WithName("DeletePost")
            .WithOpenApi()
            .WithTags("Blog")
            .Produces(204)
            .Produces(404);

        app.MapPost("/blog/reactions/",
            async ([FromForm] SendReactionApiRequest request,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var command = new SendReactionCommand(request.PostId, request.ReactionTypeId, context.GetUserId());
                var result = await sender.SendAsync(command, cancellationToken);
                return result.ToResult();
            })
            .DisableAntiforgery()
            .WithName("SendReaction")
            .WithOpenApi()
            .WithTags("Blog")
            .Produces(201)
            .Produces(400);

        app.MapPatch("/blog/pin/toggle/",
            async ([FromBody] TogglePinPostApiRequest request,
                [FromServices] ISender sender,
                HttpContext context, CancellationToken cancellationToken) =>
            {
                var command = new TogglePinPostRequest(request.PostId, context.GetUserId());
                var result = await sender.SendAsync(command, cancellationToken);
                return result.ToResult();
            })
            .WithName("TogglePinPost")
            .WithOpenApi()
            .WithTags("Blog")
            .Produces(200)
            .Produces(404);

        app.MapGet("/blog/user/{userIdentifier}/posts", async (
            [FromRoute] string userIdentifier,
            [FromQuery] int skip,
            [FromQuery] int take,
            [FromServices] ISender sender, HttpContext context,
            CancellationToken cancellationToken,
            [FromQuery] bool? desc = true) =>
        {
            var currentUserId = context.GetUserId();
            var query = new GetUserPostsByIdentifierQuery(userIdentifier, new PageSettings(skip, take), desc, currentUserId);
            var result = await sender.SendAsync(query, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("GetUserPosts")
        .WithOpenApi()
        .WithTags("Blog")
        .Produces<PagedList<PostListItemDto>>(200)
        .Produces(404);

        app.MapGet("/blog/posts/{postId:guid}/comments", async (
            [FromRoute] Guid postId,
            [FromQuery] int skip,
            [FromQuery] int take,
            [FromServices] ISender sender,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var currentUserId = context.GetUserId();
            var query = new GetPostCommentsQuery(postId, new PageSettings(skip, take), currentUserId);
            var result = await sender.SendAsync(query, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("GetPostComments")
        .WithOpenApi()
        .WithTags("Blog")
        .Produces<PagedList<CommentDto>>(200)
        .Produces(404);

        app.MapPost("/blog/posts/{postId:guid}/comments", async (
            [FromRoute] Guid postId,
            [FromForm] AddCommentForm form,
            [FromServices] ISender sender,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var command = new AddCommentRequest(postId, context.GetUserId(), form.Content, form.Attachment, form.ParentCommentId);
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

        app.MapPost("/blog/comments/{commentId:guid}/reply", async (
            [FromRoute] Guid commentId,
            [FromForm] ReplyToCommentForm form,
            [FromServices] ISender sender,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var command = new ReplyToCommentRequest(commentId, context.GetUserId(), form.Content, form.Attachment);
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

        app.MapPost("/blog/posts/{postId:guid}/toggle-like/{reactionTypeId:guid}", async (
            [FromRoute] Guid postId, [FromRoute] Guid reactionTypeId,
            [FromServices] ISender sender,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var command = new TogglePostLikeRequest(postId, context.GetUserId(), reactionTypeId);
            var result = await sender.SendAsync(command, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("TogglePostLike")
        .WithOpenApi()
        .WithTags("Blog")
        .Produces(200)
        .Produces(400)
        .Produces(404);

        app.MapPost("/blog/comments/{commentId:guid}/toggle-like/{reactionTypeId:guid}", async (
            [FromRoute] Guid commentId, [FromRoute] Guid reactionTypeId,
            [FromServices] ISender sender,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var command = new ToggleCommentLikeRequest(commentId, context.GetUserId(), reactionTypeId);
            var result = await sender.SendAsync(command, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("ToggleCommentLike")
        .WithOpenApi()
        .WithTags("Blog")
        .Produces(200)
        .Produces(400)
        .Produces(404);

        app.MapPatch("/blog/comments/toggle/{postId:guid}",
            async ([FromRoute] Guid postId,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var command = new TogglePostCommentsRequest(postId, context.GetUserId());
                var result = await sender.SendAsync(command, cancellationToken);
                return result.ToResult();
            })
            .WithName("TogglePostComments")
            .WithOpenApi()
            .WithTags("Blog")
            .Produces(200)
            .Produces(404);

        app.MapGet("/blog/posts/{postId:guid}", async (
            [FromRoute] Guid postId,
            [FromServices] ISender sender,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var currentUserId = context.GetUserId();
            var query = new GetPostByIdQuery(postId, currentUserId);
            var result = await sender.SendAsync(query, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("GetPostById")
        .WithOpenApi()
        .WithTags("Blog")
        .Produces<PostListItemDto>(200)
        .Produces(404);

        app.MapPut("/blog/{postId:guid}", async (
            [FromRoute] Guid postId,
            [FromForm] UpdatePostApiRequest request,
            [FromServices] ISender sender,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var userId = context.GetUserId();
            var command = new UpdatePostRequest
            {
                PostId = postId,
                UserId = userId,
                Title = request.Title,
                Content = request.Content,
                File = request.File
            };
            var result = await sender.SendAsync(command, cancellationToken);
            return result.ToResult();
        })
        .WithName("UpdatePost")
        .DisableAntiforgery()
        .WithOpenApi()
        .WithTags("Blog")
        .Produces(204)
        .Produces(400)
        .Produces(403)
        .Produces(404);
    }

    private class AddPostApiRequest
    {
        public IFormFile? File { get; set; }
        public string? Content { get; set; }
        public string? Title { get; set; }
    }

    private record SendReactionApiRequest(Guid PostId, Guid ReactionTypeId);
    private record TogglePinPostApiRequest(Guid PostId);

    private class UpdatePostApiRequest
    {
        public string? Title { get; set; }
        public string Content { get; set; } = null!;
        public IFormFile? File { get; set; }
    }
}