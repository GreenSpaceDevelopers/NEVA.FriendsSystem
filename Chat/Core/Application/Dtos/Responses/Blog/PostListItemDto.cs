using Swashbuckle.AspNetCore.Annotations;

namespace Application.Dtos.Responses.Blog;

/// <summary>
/// DTO элемента списка постов
/// </summary>
[SwaggerSchema(Description = "Информация о посте в списке")]
public record PostListItemDto(
    [SwaggerSchema(Description = "Уникальный идентификатор поста")]
    Guid Id,
    [SwaggerSchema(Description = "Заголовок поста")]
    string Title,
    [SwaggerSchema(Description = "Содержимое поста")]
    string Content,
    [SwaggerSchema(Description = "URL вложения к посту (может быть null)")]
    string? AttachmentUrl,
    [SwaggerSchema(Description = "Дата и время создания поста")]
    DateTime CreatedAt,
    [SwaggerSchema(Description = "Количество комментариев к посту")]
    int CommentsCount,
    [SwaggerSchema(Description = "Количество реакций к посту")]
    int ReactionsCount,
    [SwaggerSchema(Description = "Закреплен ли пост")]
    bool IsPinned,
    [SwaggerSchema(Description = "Включены ли комментарии к посту")]
    bool IsCommentsEnabled
);