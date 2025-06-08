using Domain.Abstractions;
using Domain.Models.Messaging;

namespace Domain.Models.Blog;

public class Comment : Entity<Comment>
{
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid AttachmentId { get; set; }
    public Guid AuthorId { get; set; }
    public Guid PostId { get; set; }
    public Guid ParentId { get; set; }
    
    public Comment? Parent { get; set; }
    public Post Post { get; set; } = null!;
    public Attachment? Attachment { get; set; }
    public ChatUser Author { get; set; } = null!;
}