using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Requests.Commands.Posts;
using Application.Dtos.Requests.Shared;
using Application.Requests.Queries.Blog;
using Microsoft.AspNetCore.Mvc;
using WebApi.Common.Mappers;
using Microsoft.AspNetCore.Http;
using Application.Requests.Commands.Blog;
using External.WebApi.Common.Helpers;

namespace WebApi.Endpoints;

public class AddCommentForm
{
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public IFormFile? Attachment { get; set; }
    public Guid? ParentCommentId { get; set; }
}

public class ReplyToCommentForm
{
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public IFormFile? Attachment { get; set; }
}

public static class Blog
{
    public static void MapBlogEndpoints(this WebApplication app)
    {
        app.MapPost("/blog/",
            async ([FromForm] AddPostRequest request,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);
                return result.ToResult();
            })
            .WithName("AddPost");

        app.MapDelete("/blog/",
            async ([FromBody] DeletePostRequest request,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);
                return result.ToResult();
            })
            .WithName("DeletePost");

        app.MapPost("/blog/reactions/",
            async ([FromForm] SendReactionCommand request,
                [FromServices] ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);
                return result.ToResult();
            })
            .WithName("SendReaction");

        app.MapPatch("/blog/pin/toggle/",
            async ([FromBody] TogglePinPostRequest request,
                [FromServices] ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);
                return result.ToResult();
            })
            .WithName("TogglePinPost");

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
        .WithName("GetUserPosts");

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
        .WithName("GetPostComments");

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
        .WithName("AddComment");

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
        .WithName("ReplyToComment");

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
        .WithName("TogglePostLike");

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
        .WithName("ToggleCommentLike");

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
        .WithName("TogglePostComments");
    }
}