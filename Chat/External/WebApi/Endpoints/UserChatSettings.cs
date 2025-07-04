using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Dtos.Requests.Messaging;
using Application.Dtos.Responses.Messaging;
using Application.Requests.Queries.Messaging;
using Microsoft.AspNetCore.Mvc;
using WebApi.Common.Helpers;
using WebApi.Common.Models;

namespace WebApi.Endpoints;

public static class UserChatSettings
{
    public static void MapUserChatSettingsEndpoints(this WebApplication app)
    {
        app.MapGet("/user-chat-settings", async (
            [FromServices] ISender sender,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAllUserChatSettingsQuery(context.GetUserId());
            var result = await sender.SendAsync(query, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("GetAllUserChatSettings")
        .WithOpenApi()
        .WithTags("UserChatSettings")
        .Produces<List<UserChatSettingsDto>>(200)
        .Produces<NotFoundErrorResponse>(404);

        app.MapGet("/user-chat-settings/{chatId:guid}", async (
            [FromRoute] Guid chatId,
            [FromServices] ISender sender,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var query = new GetUserChatSettingsQuery(context.GetUserId(), chatId);
            var result = await sender.SendAsync(query, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("GetUserChatSettings")
        .WithOpenApi()
        .WithTags("UserChatSettings")
        .Produces<UserChatSettingsDto>(200)
        .Produces<NotFoundErrorResponse>(404);

        app.MapPut("/user-chat-settings/{chatId:guid}", async (
            [FromRoute] Guid chatId,
            [FromBody] UpdateUserChatSettingsRequest request,
            [FromServices] ISender sender,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            request = request with { UserId = context.GetUserId(), ChatId = chatId };
            var result = await sender.SendAsync(request, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("UpdateUserChatSettings")
        .WithOpenApi()
        .WithTags("UserChatSettings")
        .Produces<UserChatSettingsDto>(200)
        .Produces<ValidationErrorResponse>(400)
        .Produces<NotFoundErrorResponse>(404);
    }
} 