using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Swagger;

/// <summary>
/// Общие аннотации для Swagger документации
/// </summary>
public static class SwaggerDocumentation
{
    /// <summary>
    /// Теги для группировки API эндпоинтов
    /// </summary>
    public static class Tags
    {
        public const string Profile = "Profile";
        public const string Blog = "Blog";
        public const string Friends = "Friends";
        public const string Chats = "Chats";
        public const string Media = "Media";
        public const string BlackList = "BlackList";
    }

    /// <summary>
    /// Описания HTTP статус кодов
    /// </summary>
    public static class StatusCodes
    {
        public const string Ok = "Операция выполнена успешно";
        public const string Created = "Ресурс успешно создан";
        public const string NoContent = "Операция выполнена успешно, но контент не возвращен";
        public const string BadRequest = "Некорректные данные запроса";
        public const string Unauthorized = "Требуется авторизация";
        public const string Forbidden = "Доступ запрещен";
        public const string NotFound = "Ресурс не найден";
        public const string Conflict = "Конфликт данных";
        public const string InternalServerError = "Внутренняя ошибка сервера";
    }

    /// <summary>
    /// Общие описания параметров
    /// </summary>
    public static class Parameters
    {
        public const string UserId = "Уникальный идентификатор пользователя";
        public const string PageNumber = "Номер страницы (начиная с 1)";
        public const string PageSize = "Размер страницы";
        public const string SearchQuery = "Строка поиска";
        public const string CancellationToken = "Токен отмены операции";
    }

    /// <summary>
    /// Примеры запросов
    /// </summary>
    public static class Examples
    {
        public static class Profile
        {
            public const string UpdateProfile = @"
{
  ""userId"": ""123e4567-e89b-12d3-a456-426614174000"",
  ""username"": ""john_doe"",
  ""privacySetting"": 2
}";

            public const string ValidateUsername = @"
{
  ""username"": ""john_doe""
}";
        }

        public static class Blog
        {
            public const string AddComment = @"
{
  ""userId"": ""123e4567-e89b-12d3-a456-426614174000"",
  ""content"": ""Отличный пост!"",
  ""parentCommentId"": null
}";

            public const string ToggleLike = @"
""123e4567-e89b-12d3-a456-426614174000""";
        }

        public static class Friends
        {
            public const string AddFriend = @"
{
  ""userId"": ""123e4567-e89b-12d3-a456-426614174000"",
  ""friendId"": ""987fcdeb-51a2-43d1-b789-123456789abc""
}";

            public const string BlockUser = @"
{
  ""userId"": ""123e4567-e89b-12d3-a456-426614174000"",
  ""blockedUserId"": ""987fcdeb-51a2-43d1-b789-123456789abc""
}";
        }
    }

    /// <summary>
    /// Примеры ответов
    /// </summary>
    public static class ResponseExamples
    {
        public static class Profile
        {
            public const string ProfileDto = @"
{
  ""id"": ""123e4567-e89b-12d3-a456-426614174000"",
  ""username"": ""john_doe"",
  ""avatarUrl"": ""https://example.com/avatar.jpg"",
  ""coverUrl"": ""https://example.com/cover.jpg"",
  ""privacySetting"": 2
}";

            public const string ValidationDto = @"
{
  ""isAvailable"": true,
  ""errorMessage"": null
}";
        }

        public static class Error
        {
            public const string NotFound = @"
{
  ""code"": ""NOT_FOUND"",
  ""message"": ""Пользователь не найден"",
  ""resourceType"": ""User"",
  ""resourceId"": ""123e4567-e89b-12d3-a456-426614174000""
}";

            public const string Validation = @"
{
  ""code"": ""VALIDATION_ERROR"",
  ""message"": ""Некорректные данные"",
  ""fieldErrors"": {
    ""username"": [""Имя пользователя должно содержать от 3 до 50 символов""]
  }
}";
        }
    }
}

/// <summary>
/// Атрибуты для стандартизации Swagger документации
/// </summary>
public static class SwaggerAttributes
{
    /// <summary>
    /// Стандартные ответы для успешных операций
    /// </summary>
    public static class Responses
    {
        public static SwaggerResponseAttribute Ok(string description = "Операция выполнена успешно") =>
            new(200, description);

        public static SwaggerResponseAttribute Created(string description = "Ресурс успешно создан") =>
            new(201, description);

        public static SwaggerResponseAttribute NoContent(string description = "Операция выполнена успешно") =>
            new(204, description);

        public static SwaggerResponseAttribute BadRequest(string description = "Некорректные данные") =>
            new(400, description);

        public static SwaggerResponseAttribute Unauthorized(string description = "Требуется авторизация") =>
            new(401, description);

        public static SwaggerResponseAttribute Forbidden(string description = "Доступ запрещен") =>
            new(403, description);

        public static SwaggerResponseAttribute NotFound(string description = "Ресурс не найден") =>
            new(404, description);

        public static SwaggerResponseAttribute Conflict(string description = "Конфликт данных") =>
            new(409, description);

        public static SwaggerResponseAttribute InternalServerError(string description = "Внутренняя ошибка сервера") =>
            new(500, description);
    }

    /// <summary>
    /// Стандартные операции
    /// </summary>
    public static class Operations
    {
        public static SwaggerOperationAttribute Get(string summary, string description = "", string[]? tags = null) =>
            new()
            {
                Summary = summary,
                Description = description,
                Tags = tags ?? Array.Empty<string>()
            };

        public static SwaggerOperationAttribute Post(string summary, string description = "", string[]? tags = null) =>
            new()
            {
                Summary = summary,
                Description = description,
                Tags = tags ?? Array.Empty<string>()
            };

        public static SwaggerOperationAttribute Put(string summary, string description = "", string[]? tags = null) =>
            new()
            {
                Summary = summary,
                Description = description,
                Tags = tags ?? Array.Empty<string>()
            };

        public static SwaggerOperationAttribute Delete(string summary, string description = "", string[]? tags = null) =>
            new()
            {
                Summary = summary,
                Description = description,
                Tags = tags ?? Array.Empty<string>()
            };
    }
} 