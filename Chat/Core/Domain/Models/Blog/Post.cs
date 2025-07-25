using Domain.Abstractions;
using Domain.Models.Messaging;
using Domain.Models.Users;

namespace Domain.Models.Blog;

public class Post : Entity<Post>
{
    public string? Title { get; set; }
    public string Content { get; set; } = null!;
    public bool IsRepost { get; set; }
    public bool IsPinned { get; set; } = false;
    public Guid? AttachmentId { get; set; }
    public Guid? OriginalPostId { get; set; }
    public Guid AuthorId { get; set; }
    public DateTime CreatedAt { get; set; }

    public ChatUser Author { get; set; } = null!;
    public Post? OriginalPost { get; set; }
    public Attachment? Attachment { get; set; }

    public List<PostReaction> Reactions { get; set; } = [];
    public List<Comment> Comments { get; set; } = [];
    public bool IsCommentsEnabled { get; set; } = true;
}

