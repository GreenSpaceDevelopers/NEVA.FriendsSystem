using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Requests.Commands.Profile;
using Application.Requests.Queries.Profile;
using Domain.Models.Users;
using Microsoft.AspNetCore.Mvc;
using WebApi.Common.Helpers;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Common.Models;

namespace WebApi.Endpoints;

public static class Profile
{
    public static void MapProfileEndpoints(this WebApplication app)
    {
        app.MapGet("/profile/{userId:guid}", async (
            [FromRoute] Guid userId,
            [FromServices] ISender sender, HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var query = new GetUserProfileQuery(userId, context.GetUserId());
            var result = await sender.SendAsync(query, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("GetUserProfile")
        .WithOpenApi()
        .WithTags("Profile")
        .Produces<Application.Dtos.Responses.Profile.ProfileDto>(200)
        .Produces<NotFoundErrorResponse>(404);

        app.MapGet("/profile/link/{personalLink}", async (
            [FromRoute] string personalLink,
            [FromServices] ISender sender, HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var query = new GetUserByPersonalLinkQuery(personalLink, context.GetUserId());
            var result = await sender.SendAsync(query, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("GetUserProfileByPersonalLink")
        .WithOpenApi()
        .WithTags("Profile")
        .Produces<Application.Dtos.Responses.Profile.ProfileDto>(200)
        .Produces<NotFoundErrorResponse>(404);

        app.MapGet("/profile", async (
            [FromServices] ISender sender,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var currentUserId = context.GetUserId();
            if (currentUserId == Guid.Empty)
            {
                return Results.Unauthorized();
            }

            var query = new GetOwnProfileQuery(currentUserId);
            var result = await sender.SendAsync(query, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("GetCurrentUserProfileNoSlash")
        .WithOpenApi()
        .WithTags("Profile")
        .Produces<Application.Dtos.Responses.Profile.OwnProfileDto>(200)
        .Produces<NotFoundErrorResponse>(404);

        app.MapPut("/profile/", async (
            [FromForm] UpdateProfileForm form,
            [FromServices] ISender sender, HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var request = new UpdateProfileRequest(
                context.GetUserId(),
                form.Username,
                form.PersonalLink,
                form.Name,
                form.Surname,
                form.MiddleName,
                form.DateOfBirth,
                form.Avatar,
                form.Cover);
            
            var result = await sender.SendAsync(request, cancellationToken);
            
            return result.ToApiResult();
        })
        .DisableAntiforgery()
        .WithName("UpdateProfile")
        .WithOpenApi()
        .WithTags("Profile")
        .Produces(204)
        .Produces<ValidationErrorResponse>(400)
        .Produces<NotFoundErrorResponse>(404);

        app.MapPost("/profile/validate-username", async (
            [FromBody] ValidateUsernameRequest request,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(request, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("ValidateUsername")
        .WithOpenApi()
        .WithTags("Profile")
        .Produces<Application.Dtos.Responses.Profile.ProfileValidationDto>(200)
        .Produces<ValidationErrorResponse>(400);

        app.MapPost("/profile/validate-personal-link", async (
            [FromBody] ValidatePersonalLinkRequest request,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.SendAsync(request, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("ValidatePersonalLink")
        .WithOpenApi()
        .WithTags("Profile")
        .Produces<Application.Dtos.Responses.Profile.ProfileValidationDto>(200)
        .Produces<ValidationErrorResponse>(400);

        app.MapGet("/profile/{userId:guid}/permissions", async (
            [FromRoute] Guid userId,
            [FromServices] ISender sender,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var query = new GetUserInteractionPermissionsQuery(userId, context.GetUserId());
            var result = await sender.SendAsync(query, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("GetUserInteractionPermissions")
        .WithOpenApi()
        .WithTags("Profile")
        .Produces<Application.Dtos.Responses.Profile.UserInteractionPermissionsDto>(200)
        .Produces<NotFoundErrorResponse>(404);
    }

    /// <summary>
    /// Форма для обновления профиля пользователя
    /// </summary>
    public class UpdateProfileForm
    {
        /// <summary>
        /// Имя пользователя
        /// </summary>
        [SwaggerSchema(Description = "Имя пользователя (от 3 до 50 символов)")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Персональная ссылка
        /// </summary>
        [SwaggerSchema(Description = "Персональная ссылка профиля (от 3 до 50 символов)")]
        public string? PersonalLink { get; set; }

        /// <summary>
        /// Имя
        /// </summary>
        [SwaggerSchema(Description = "Имя пользователя")]
        public string? Name { get; set; }

        /// <summary>
        /// Фамилия
        /// </summary>
        [SwaggerSchema(Description = "Фамилия пользователя")]
        public string? Surname { get; set; }

        /// <summary>
        /// Отчество
        /// </summary>
        [SwaggerSchema(Description = "Отчество пользователя")]
        public string? MiddleName { get; set; }

        /// <summary>
        /// Дата рождения
        /// </summary>
        [SwaggerSchema(Description = "Дата рождения пользователя")]
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// Аватар пользователя
        /// </summary>
        [SwaggerSchema(Description = "Файл аватара (изображение)")]
        public IFormFile? Avatar { get; set; }

        /// <summary>
        /// Обложка профиля
        /// </summary>
        [SwaggerSchema(Description = "Файл обложки профиля (изображение)")]
        public IFormFile? Cover { get; set; }
    }
}