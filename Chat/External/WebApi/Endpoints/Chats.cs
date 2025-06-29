using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Dtos.Requests.Shared;
using Application.Requests.Queries.Messaging;
using Microsoft.AspNetCore.Mvc;
using External.WebApi.Common.Helpers;
using WebApi.Common.Mappers;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Endpoints;

public static class Chats
{
    /// <summary>
    /// Получить чаты пользователя
    /// </summary>
    [SwaggerOperation(
        Summary = "Получить чаты пользователя",
        Description = "Возвращает список чатов пользователя с пагинацией",
        OperationId = "GetUserChats",
        Tags = new[] { "Chats" }
    )]
    [SwaggerResponse(200, "Список чатов получен", typeof(List<Application.Dtos.Responses.Chats.UserChatListItemDto>))]
    [SwaggerResponse(404, "Пользователь не найден")]
    public static void MapChatsEndpoints(this WebApplication app)
    {
        app.MapGet("/users/chats/page={page:int}/size={size:int}",
            async ([FromRoute]ushort page, [FromRoute]ushort size, 
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var query = new GetAllChatsForUserQuery(context.GetUserId(), new PageSettings(page, size));
                var result = await sender.SendAsync(query, cancellationToken);
                
                return result.ToResult();
            })
            .WithName("GetUserChats")
            .WithOpenApi()
            .WithTags("Chats")
            .Produces<List<Application.Dtos.Responses.Chats.UserChatListItemDto>>(200)
            .Produces(404);
    }
}