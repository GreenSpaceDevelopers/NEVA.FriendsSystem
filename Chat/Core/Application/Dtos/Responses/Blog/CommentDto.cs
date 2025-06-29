using Swashbuckle.AspNetCore.Annotations;

namespace Application.Dtos.Responses.Blog;

/// <summary>
/// DTO комментария
/// </summary>
[SwaggerSchema(Description = "Информация о комментарии")]
public record CommentDto(
    /// <summary>
    /// Уникальный идентификатор комментария
    /// </summary>
    [SwaggerSchema(Description = "Уникальный идентификатор комментария")]
    Guid Id,
    
    /// <summary>
    /// Содержимое комментария
    /// </summary>
    [SwaggerSchema(Description = "Текст комментария")]
    string Content,
    
    /// <summary>
    /// URL вложения к комментарию
    /// </summary>
    [SwaggerSchema(Description = "URL вложения к комментарию (может быть null)")]
    string? AttachmentUrl,
    
    /// <summary>
    /// Дата создания комментария
    /// </summary>
    [SwaggerSchema(Description = "Дата и время создания комментария")]
    DateTime CreatedAt,
    
    /// <summary>
    /// ID автора комментария
    /// </summary>
    [SwaggerSchema(Description = "Уникальный идентификатор автора комментария")]
    Guid AuthorId,
    
    /// <summary>
    /// Имя автора комментария
    /// </summary>
    [SwaggerSchema(Description = "Имя пользователя автора комментария")]
    string AuthorUsername,
    
    /// <summary>
    /// URL аватара автора
    /// </summary>
    [SwaggerSchema(Description = "URL аватара автора комментария (может быть null)")]
    string? AuthorAvatarUrl,
    
    /// <summary>
    /// ID родительского комментария
    /// </summary>
    [SwaggerSchema(Description = "ID родительского комментария для ответов (может быть null)")]
    Guid? ParentCommentId,
    
    /// <summary>
    /// Количество ответов на комментарий
    /// </summary>
    [SwaggerSchema(Description = "Количество ответов на данный комментарий")]
    int RepliesCount,
    
    /// <summary>
    /// Количество реакций на комментарий
    /// </summary>
    [SwaggerSchema(Description = "Количество реакций на комментарий")]
    int ReactionsCount); 