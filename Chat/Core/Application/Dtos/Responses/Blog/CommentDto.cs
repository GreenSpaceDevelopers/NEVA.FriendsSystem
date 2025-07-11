using Swashbuckle.AspNetCore.Annotations;

namespace Application.Dtos.Responses.Blog;

/// <summary>
/// DTO комментария
/// </summary>
[SwaggerSchema(Description = "Информация о комментарии")]
public record CommentDto(
    [SwaggerSchema(Description = "Уникальный идентификатор комментария")]
    Guid Id,
    [SwaggerSchema(Description = "Текст комментария")]
    string Content,
    [SwaggerSchema(Description = "URL вложения к комментарию (может быть null)")]
    string? AttachmentUrl,
    [SwaggerSchema(Description = "Дата и время создания комментария")]
    DateTime CreatedAt,
    [SwaggerSchema(Description = "Уникальный идентификатор автора комментария")]
    Guid AuthorId,
    [SwaggerSchema(Description = "Имя пользователя автора комментария")]
    string AuthorUsername,
    [SwaggerSchema(Description = "Персональная ссылка профиля автора (slug)")]
    string AuthorSlug,
    [SwaggerSchema(Description = "URL аватара автора комментария (может быть null)")]
    string? AuthorAvatarUrl,
    [SwaggerSchema(Description = "ID родительского комментария для ответов (может быть null)")]
    Guid? ParentCommentId,
    [SwaggerSchema(Description = "Количество ответов на данный комментарий")]
    int RepliesCount,
    [SwaggerSchema(Description = "Количество реакций на комментарий")]
    int ReactionsCount,
    [SwaggerSchema(Description = "Лайкнул ли текущий пользователь этот комментарий")]
    bool IsLikedByCurrentUser,
    [SwaggerSchema(Description = "Вложенные ответы на комментарий")]
    List<CommentDto> Replies
);