using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Requests.Commands.Profile;
using Application.Requests.Queries.Profile;
using Domain.Models.Users;
using Microsoft.AspNetCore.Mvc;
using WebApi.Common.Mappers;
using External.WebApi.Common.Helpers;
using Swashbuckle.AspNetCore.Annotations;
using WebApi.Common.Models;

namespace WebApi.Endpoints;

public static class Profile
{
    /// <summary>
    /// Получить профиль пользователя
    /// </summary>
    /// <param name="userId">ID пользователя, профиль которого нужно получить</param>
    /// <param name="currentUserId">ID текущего пользователя</param>
    /// <param name="sender">Сервис для отправки запросов</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Профиль пользователя</returns>
    [SwaggerOperation(
        Summary = "Получить профиль пользователя",
        Description = "Возвращает профиль пользователя с учетом настроек приватности",
        OperationId = "GetUserProfile",
        Tags = new[] { "Profile" }
    )]
    [SwaggerResponse(200, "Профиль успешно получен", typeof(Application.Dtos.Responses.Profile.ProfileDto))]
    [SwaggerResponse(404, "Пользователь не найден", typeof(NotFoundErrorResponse))]
    public static void MapProfileEndpoints(this WebApplication app)
    {
        app.MapGet("/profile/{userId:guid}", async (
            [FromRoute] Guid userId,
            [FromQuery] Guid currentUserId,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetUserProfileQuery(userId, currentUserId);
            var result = await sender.SendAsync(query, cancellationToken);
            return result.ToApiResult();
        })
        .WithName("GetUserProfile")
        .WithOpenApi()
        .WithTags("Profile")
        .Produces<Application.Dtos.Responses.Profile.ProfileDto>(200)
        .Produces<NotFoundErrorResponse>(404);

        /// <summary>
        /// Обновить профиль пользователя
        /// </summary>
        /// <param name="form">Данные для обновления профиля</param>
        /// <param name="sender">Сервис для отправки запросов</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат обновления</returns>
        [SwaggerOperation(
            Summary = "Обновить профиль пользователя",
            Description = "Обновляет информацию профиля пользователя, включая аватар и обложку",
            OperationId = "UpdateProfile",
            Tags = new[] { "Profile" }
        )]
        [SwaggerResponse(204, "Профиль успешно обновлен")]
        [SwaggerResponse(400, "Некорректные данные", typeof(ValidationErrorResponse))]
        [SwaggerResponse(404, "Пользователь не найден", typeof(NotFoundErrorResponse))]
        app.MapPut("/profile/", async (
            [FromForm] UpdateProfileForm form,
            [FromServices] ISender sender,
            CancellationToken cancellationToken) =>
        {
            var request = new UpdateProfileRequest(form.UserId, form.Username, form.Avatar, form.Cover, form.PrivacySetting);
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

        /// <summary>
        /// Проверить доступность имени пользователя
        /// </summary>
        /// <param name="request">Запрос на валидацию имени пользователя</param>
        /// <param name="sender">Сервис для отправки запросов</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат валидации</returns>
        [SwaggerOperation(
            Summary = "Проверить доступность имени пользователя",
            Description = "Проверяет, доступно ли указанное имя пользователя для регистрации",
            OperationId = "ValidateUsername",
            Tags = new[] { "Profile" }
        )]
        [SwaggerResponse(200, "Результат валидации", typeof(Application.Dtos.Responses.Profile.ProfileValidationDto))]
        [SwaggerResponse(400, "Некорректные данные", typeof(ValidationErrorResponse))]
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
    }

    /// <summary>
    /// Форма для обновления профиля пользователя
    /// </summary>
    public class UpdateProfileForm
    {
        /// <summary>
        /// ID пользователя
        /// </summary>
        [SwaggerSchema(Description = "Уникальный идентификатор пользователя")]
        public Guid UserId { get; set; }
        
        /// <summary>
        /// Имя пользователя
        /// </summary>
        [SwaggerSchema(Description = "Имя пользователя (от 3 до 50 символов)")]
        public string Username { get; set; } = string.Empty;
        
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
        
        /// <summary>
        /// Настройки приватности
        /// </summary>
        [SwaggerSchema(Description = "Настройки приватности профиля")]
        public PrivacySettingsEnums PrivacySetting { get; set; }
    }
} 