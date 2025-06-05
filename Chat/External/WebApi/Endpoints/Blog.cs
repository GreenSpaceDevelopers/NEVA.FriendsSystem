using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Requests.Commands.Posts;
using Microsoft.AspNetCore.Mvc;
using WebApi.Common.Mappers;

namespace WebApi.Endpoints;

public static class Blog
{
    public static void MapBlogEndpoints(this WebApplication app)
    {
        app.MapPost("/blog/",
            async ([FromForm] AddPostRequest request, 
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);
                
                return result.ToResult();
            });
        
        app.MapDelete("/blog/",
            async ([FromBody] DeletePostRequest request, 
                [FromServices]ISender sender, HttpContext context, CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);
                
                return result.ToResult();
            });
    }
}