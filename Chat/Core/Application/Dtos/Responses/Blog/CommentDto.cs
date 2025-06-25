namespace Application.Dtos.Responses.Blog;

public record CommentDto(
    Guid Id,
    string Content,
    string? AttachmentUrl,
    DateTime CreatedAt,
    Guid AuthorId,
    string AuthorUsername,
    string? AuthorAvatarUrl,
    Guid? ParentCommentId,
    int RepliesCount,
    int LikesCount
); 