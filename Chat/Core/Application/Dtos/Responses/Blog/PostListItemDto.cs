using Swashbuckle.AspNetCore.Annotations;

namespace Application.Dtos.Responses.Blog;

/// <summary>
/// DTO элемента списка постов
/// </summary>
[SwaggerSchema(Description = "Информация о посте в списке")]
public record PostListItemDto(
    /// <summary>
    /// Уникальный идентификатор поста
    /// </summary>
    [SwaggerSchema(Description = "Уникальный идентификатор поста")]
    Guid Id,
    
    /// <summary>
    /// Заголовок поста
    /// </summary>
    [SwaggerSchema(Description = "Заголовок поста")]
    string Title,
    
    /// <summary>
    /// Содержимое поста
    /// </summary>
    [SwaggerSchema(Description = "Содержимое поста")]
    string Content,
    
    /// <summary>
    /// URL вложения к посту
    /// </summary>
    [SwaggerSchema(Description = "URL вложения к посту (может быть null)")]
    string? AttachmentUrl,
    
    /// <summary>
    /// Дата создания поста
    /// </summary>
    [SwaggerSchema(Description = "Дата и время создания поста")]
    DateTime CreatedAt,
    
    /// <summary>
    /// Количество комментариев
    /// </summary>
    [SwaggerSchema(Description = "Количество комментариев к посту")]
    int CommentsCount,
    
    /// <summary>
    /// Количество реакций
    /// </summary>
    [SwaggerSchema(Description = "Количество реакций к посту")]
    int ReactionsCount,
    
    /// <summary>
    /// Закреплен ли пост
    /// </summary>
    [SwaggerSchema(Description = "Закреплен ли пост")]
    bool IsPinned,
    
    /// <summary>
    /// Включены ли комментарии
    /// </summary>
    [SwaggerSchema(Description = "Включены ли комментарии к посту")]
    bool IsCommentsEnabled
); 