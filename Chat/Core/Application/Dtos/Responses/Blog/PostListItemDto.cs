namespace Application.Dtos.Responses.Blog;

public record PostListItemDto(
    Guid Id,
    string Title,
    string Content,
    string? AttachmentUrl,
    DateTime CreatedAt,
    int CommentsCount,
    int LikesCount,
    bool IsPinned,
    bool IsCommentsEnabled
); 