using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Dtos.Requests.Shared;
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
        .Produces<List<Application.Dtos.Responses.Chats.UserChatListItemDto>>(200)
        .Produces(404);

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
        .Produces<List<Application.Dtos.Responses.Chats.MessageDto>>(200)
        .Produces(404);
    }
}