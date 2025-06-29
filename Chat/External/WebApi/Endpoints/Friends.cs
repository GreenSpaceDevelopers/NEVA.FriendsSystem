using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Dtos.Requests.Shared;
using Application.Requests.Commands.Friends;
using Application.Requests.Queries.BlackList;
using Application.Requests.Queries.Friends;
using Microsoft.AspNetCore.Mvc;
using External.WebApi.Common.Helpers;
using WebApi.Common.Mappers;
using Swashbuckle.AspNetCore.Annotations;

namespace External.WebApi.Endpoints;

public static class Friends
{
    /// <summary>
    /// Добавить друга
    /// </summary>
    [SwaggerOperation(
        Summary = "Добавить друга",
        Description = "Отправляет запрос на добавление в друзья",
        OperationId = "AddFriend",
        Tags = new[] { "Friends" }
    )]
    [SwaggerResponse(201, "Запрос на добавление в друзья отправлен")]
    [SwaggerResponse(400, "Некорректные данные")]
    [SwaggerResponse(404, "Пользователь не найден")]
    public static void MapFriendsEndpoints(this WebApplication app)
    {
        app.MapPost("/friends/",
            async ([FromBody] AddFriendRequest request, 
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);
                
                return result.ToResult();
            })
            .WithName("AddFriend")
            .WithOpenApi()
            .WithTags("Friends")
            .Produces(201)
            .Produces(400)
            .Produces(404);
        
        /// <summary>
        /// Удалить друга
        /// </summary>
        [SwaggerOperation(
            Summary = "Удалить друга",
            Description = "Удаляет пользователя из списка друзей",
            OperationId = "DeleteFriend",
            Tags = new[] { "Friends" }
        )]
        [SwaggerResponse(204, "Друг успешно удален")]
        [SwaggerResponse(404, "Пользователь не найден")]
        app.MapDelete("/friends/",
            async ([FromBody] DeleteFriendRequest request, 
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);
                
                return result.ToResult();
            })
            .WithName("DeleteFriend")
            .WithOpenApi()
            .WithTags("Friends")
            .Produces(204)
            .Produces(404);
        
        /// <summary>
        /// Получить черный список пользователя
        /// </summary>
        [SwaggerOperation(
            Summary = "Получить черный список",
            Description = "Возвращает список заблокированных пользователей с пагинацией",
            OperationId = "GetUserBlackList",
            Tags = new[] { "Friends" }
        )]
        [SwaggerResponse(200, "Список заблокированных пользователей получен", typeof(List<Application.Dtos.Responses.BlackList.BlackListItemDto>))]
        app.MapGet("/friends/blacklist/page={page:int}/size={size:int}",
            async ([FromRoute]ushort page, [FromRoute]ushort size, [FromQuery] string searchQuery, 
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var query = new GetUserBlackListQuery(context.GetUserId(), searchQuery, new PageSettings(page, size));
                var result = await sender.SendAsync(query, cancellationToken);
                
                return result.ToResult();
            })
            .WithName("GetUserBlackList")
            .WithOpenApi()
            .WithTags("Friends")
            .Produces<List<Application.Dtos.Responses.BlackList.BlackListItemDto>>(200);
        
        /// <summary>
        /// Принять запрос в друзья
        /// </summary>
        [SwaggerOperation(
            Summary = "Принять запрос в друзья",
            Description = "Принимает входящий запрос на добавление в друзья",
            OperationId = "AcceptFriendRequest",
            Tags = new[] { "Friends" }
        )]
        [SwaggerResponse(200, "Запрос в друзья принят")]
        [SwaggerResponse(404, "Запрос не найден")]
        app.MapPost("/friends/accept/",
            async ([FromBody] AcceptPendingFriendRequest request,
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);
                
                return result.ToResult();
            })
            .WithName("AcceptFriendRequest")
            .WithOpenApi()
            .WithTags("Friends")
            .Produces(200)
            .Produces(404);
        
        /// <summary>
        /// Отклонить запрос в друзья
        /// </summary>
        [SwaggerOperation(
            Summary = "Отклонить запрос в друзья",
            Description = "Отклоняет входящий запрос на добавление в друзья",
            OperationId = "DenyFriendRequest",
            Tags = new[] { "Friends" }
        )]
        [SwaggerResponse(200, "Запрос в друзья отклонен")]
        [SwaggerResponse(404, "Запрос не найден")]
        app.MapPost("/friends/deny/",
            async ([FromBody] DenyPendingFriendRequest request,
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);
                
                return result.ToResult();
            })
            .WithName("DenyFriendRequest")
            .WithOpenApi()
            .WithTags("Friends")
            .Produces(200)
            .Produces(404);
        
        /// <summary>
        /// Заблокировать пользователя
        /// </summary>
        [SwaggerOperation(
            Summary = "Заблокировать пользователя",
            Description = "Добавляет пользователя в черный список",
            OperationId = "BlockUser",
            Tags = new[] { "Friends" }
        )]
        [SwaggerResponse(200, "Пользователь заблокирован")]
        [SwaggerResponse(400, "Некорректные данные")]
        [SwaggerResponse(404, "Пользователь не найден")]
        app.MapPost("/friends/blacklist/",
            async ([FromBody] BlockUserRequest request, 
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);
                
                return result.ToResult();
            })
            .WithName("BlockUser")
            .WithOpenApi()
            .WithTags("Friends")
            .Produces(200)
            .Produces(400)
            .Produces(404);
        
        /// <summary>
        /// Разблокировать пользователя
        /// </summary>
        [SwaggerOperation(
            Summary = "Разблокировать пользователя",
            Description = "Удаляет пользователя из черного списка",
            OperationId = "RemoveFromBlockList",
            Tags = new[] { "Friends" }
        )]
        [SwaggerResponse(200, "Пользователь разблокирован")]
        [SwaggerResponse(404, "Пользователь не найден")]
        app.MapDelete("/friends/blacklist/",
            async ([FromBody] RemoveFromBlockList request,
                [FromServices]ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);
                
                return result.ToResult();
            })
            .WithName("RemoveFromBlockList")
            .WithOpenApi()
            .WithTags("Friends")
            .Produces(200)
            .Produces(404);

        /// <summary>
        /// Получить список друзей
        /// </summary>
        [SwaggerOperation(
            Summary = "Получить список друзей",
            Description = "Возвращает список друзей пользователя с пагинацией",
            OperationId = "GetFriendsList",
            Tags = new[] { "Friends" }
        )]
        [SwaggerResponse(200, "Список друзей получен", typeof(List<Application.Dtos.Responses.Friends.FriendDto>))]
        [SwaggerResponse(404, "Пользователь не найден")]
        app.MapGet("/friends/", async (
            [FromQuery] Guid userId,
            [FromQuery] ushort pageNumber,
            [FromQuery] ushort pageSize,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetFriendsListQuery(userId, new PageSettings(pageNumber, pageSize));
            var result = await sender.SendAsync(query, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("GetFriendsList")
        .WithOpenApi()
        .WithTags("Friends")
        .Produces<List<Application.Dtos.Responses.Friends.FriendDto>>(200)
        .Produces(404);
    }
}