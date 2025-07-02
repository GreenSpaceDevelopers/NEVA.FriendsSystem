using Application.Requests.Commands.Privacy;
using Application.Requests.Queries.Privacy;
using WebApi.Common.Helpers;
using Domain.Models.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Endpoints;

public static class Privacy
{
    public static void MapPrivacyEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/privacy")
            .WithTags("Privacy");

        group.MapGet("/settings", async (
            [FromServices] ISender sender,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var userId = context.GetUserId();
            var query = new GetUserPrivacySettingsQuery(userId);
            var result = await sender.SendAsync(query, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("GetUserPrivacySettings")
        .WithSummary("Получить настройки приватности текущего пользователя")
        .Produces<Application.Dtos.Responses.Profile.UserPrivacySettingsDto>(200)
        .ProducesValidationProblem(400)
        .Produces(401);

        group.MapPatch("/settings", async (
            [FromBody] UpdateUserPrivacySettingsApiRequest request,
            [FromServices] ISender sender,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var userId = context.GetUserId();
            var command = new UpdateUserPrivacySettingsCommand(
                userId,
                request.FriendsListVisibility,
                request.CommentsPermission,
                request.DirectMessagesPermission
            );
            var result = await sender.SendAsync(command, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("UpdateUserPrivacySettings")
        .WithSummary("Обновить настройки приватности")
        .WithDescription("Обновляет настройки приватности для текущего авторизованного пользователя. Можно передать только те поля, которые нужно изменить.")
        .Produces(204)
        .ProducesValidationProblem(400)
        .Produces(401);
    }
}

[SwaggerSchema(Description = "Запрос для обновления настроек приватности")]
public record UpdateUserPrivacySettingsApiRequest(
    [SwaggerSchema(Description = "Кто может видеть список друзей (0=Private, 1=Friends, 2=Public)")]
    PrivacyLevel? FriendsListVisibility,
    [SwaggerSchema(Description = "Кто может оставлять комментарии (0=Private, 1=Friends, 2=Public)")]
    PrivacyLevel? CommentsPermission,
    [SwaggerSchema(Description = "Кто может отправлять личные сообщения (0=Private, 1=Friends, 2=Public)")]
    PrivacyLevel? DirectMessagesPermission
); 