using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Dtos.Requests.Shared;
using Application.Requests.Queries.Messaging;
using Microsoft.AspNetCore.Mvc;
using External.WebApi.Common.Helpers;
using WebApi.Common.Mappers;

namespace WebApi.Endpoints;

public static class Chats
{
    public static void MapChatsEndpoints(this WebApplication app)
    {
        app.MapGet("/users/chats/page={page:int}/size={size:int}",
            async ([FromRoute]ushort page, [FromRoute]ushort size, 
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var query = new GetAllChatsForUserQuery(context.GetUserId(), new PageSettings(page, size));
                var result = await sender.SendAsync(query, cancellationToken);
                
                return result.ToResult();
            });
    }
}