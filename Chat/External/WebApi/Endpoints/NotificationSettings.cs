using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Dtos.Requests.Profile;
using Application.Dtos.Responses.Profile;
using Application.Requests.Commands.Profile;
using Application.Requests.Queries.Profile;
using Microsoft.AspNetCore.Mvc;
using WebApi.Common.Helpers;
using WebApi.Common.Mappers;
using WebApi.Common.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Endpoints;

public static class NotificationSettings
{
    public static void MapNotificationSettingsEndpoints(this WebApplication app)
    {
        app.MapGet("/notification-settings", async (
            [FromServices] ISender sender,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var query = new GetNotificationSettingsQuery(context.GetUserId());
            var result = await sender.SendAsync(query, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("GetNotificationSettings")
        .WithOpenApi()
        .WithTags("NotificationSettings")
        .Produces<NotificationSettingsDto>(200)
        .Produces(404);

        app.MapPut("/notification-settings", async (
            [FromBody] UpdateNotificationSettingsRequest request,
            [FromServices] ISender sender,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            request = request with { UserId = context.GetUserId() };
            var command = new UpdateNotificationSettingsCommand(request);
            var result = await sender.SendAsync(command, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("UpdateNotificationSettings")
        .WithOpenApi()
        .WithTags("NotificationSettings")
        .Produces<NotificationSettingsDto>(200)
        .Produces(400)
        .Produces(404);
    }
} 