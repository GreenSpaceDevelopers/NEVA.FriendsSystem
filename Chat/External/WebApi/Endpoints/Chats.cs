using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Common.Models;
using Application.Dtos.Requests.Shared;
using Application.Dtos.Responses.Chats;
using Application.Requests.Commands.Chats;
using Application.Requests.Commands.Messaging;
using Application.Requests.Queries.Messaging;
using Microsoft.AspNetCore.Mvc;
using WebApi.Common.Helpers;
using WebApi.Common.Mappers;

namespace WebApi.Endpoints;

public static class Chats
{
    public static void MapChatsEndpoints(this WebApplication app)
    {
        app.MapGet("/users/chats", async (
            [FromQuery] int skip,
            [FromQuery] int take,
            [FromServices] ISender sender,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAllChatsForUserQuery(context.GetUserId(), new PageSettings(skip, take));
            var result = await sender.SendAsync(query, cancellationToken);

            return result.ToResult();
        })
        .WithName("GetUserChats")
        .WithOpenApi()
        .WithTags("Chats")
        .Produces<PagedList<UserChatListItemDto>>(200)
        .Produces(404);

        app.MapPost("/chats/", async (
                [FromBody] Guid[] users,
                [FromServices] ISender sender,
                CancellationToken cancellationToken
            ) =>
            {
                var request = new CreateChatRequest(users);
                var result = await sender.SendAsync(request, cancellationToken);
                return result.ToResult();
            })
            .WithName("CreateChat")
            .WithOpenApi()
            .WithTags("Chats");

        app.MapGet("/chats/{chatId:guid}/messages", async (
            [FromRoute] Guid chatId,
            [FromQuery] int skip,
            [FromQuery] int take,
            [FromServices] ISender sender,
            CancellationToken cancellationToken,
            [FromQuery] bool desc = true) =>
        {
            var query = new GetChatMessagesQuery(chatId, new PageSettings(skip, take), desc);
            var result = await sender.SendAsync(query, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("GetChatMessages")
        .WithOpenApi()
        .WithTags("Chats")
        .Produces<PagedList<MessageDto>>(200)
        .Produces(404);

        app.MapPost("/chats/{chatId:guid}/messages", async (
            [FromRoute] Guid chatId,
            [FromForm] string? content,
            [FromForm] IFormFile? attachment,
            [FromServices] ISender sender,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var command = new SendMessageCommand(chatId, context.GetUserId(), content, attachment);
            var result = await sender.SendAsync(command, cancellationToken);
            return result.ToResult();
        })
        .WithName("SendChatMessage")
        .WithOpenApi()
        .WithTags("Chats")
        .Accepts<IFormFile>("multipart/form-data")
        .Produces<Guid>(201)
        .Produces(400)
        .Produces(403)
        .Produces(404);

        app.MapGet("/chats/{chatId:guid}/messages/unread", async (
            [FromRoute] Guid chatId,
            [FromQuery] int skip,
            [FromQuery] int take,
            [FromServices] ISender sender,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var query = new GetUnreadMessagesQuery(chatId, context.GetUserId(), new PageSettings(skip, take));
            var result = await sender.SendAsync(query, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("GetUnreadMessages")
        .WithOpenApi()
        .WithTags("Chats")
        .Produces<PagedList<MessageDto>>(200)
        .Produces(404);
    }
}