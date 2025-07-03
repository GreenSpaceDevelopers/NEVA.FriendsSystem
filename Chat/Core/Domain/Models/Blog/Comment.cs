using Domain.Abstractions;
using Domain.Models.Messaging;
using Domain.Models.Users;

namespace Domain.Models.Blog;

public class Comment : Entity<Comment>
{
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid? AttachmentId { get; set; }
    public Guid AuthorId { get; set; }
    public Guid PostId { get; set; }
    public Guid? ParentCommentId { get; set; }

    public Comment? Parent { get; set; }
    public List<Comment> Replies { get; set; } = [];
    public Post Post { get; set; } = null!;
    public Attachment? Attachment { get; set; }
    public ChatUser Author { get; set; } = null!;
    public List<Reaction> Reactions { get; set; } = [];
    public List<CommentReaction> CommentReactions { get; set; } = [];
}