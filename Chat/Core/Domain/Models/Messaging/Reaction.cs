using Domain.Abstractions;
using Domain.Models.Blog;
using Domain.Models.Users;

namespace Domain.Models.Messaging;

public class MessageReaction : Entity<MessageReaction>
{
    public Guid MessageId { get; set; }
    public Guid ReactorId { get; set; }
    public Guid ReactionTypeId { get; set; }
    public DateTime CreatedAt { get; set; }

    public ReactionType ReactionType { get; set; } = null!;
    public Message Message { get; set; } = null!;
    public ChatUser Reactor { get; set; } = null!;
}

public class ReactionType : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public Guid IconId { get; set; }
    public string Description { get; set; } = null!;
    public Attachment Attachment { get; set; } = null!;
}

public class PostReaction : Entity<PostReaction>
{
    public Guid PostId { get; set; }
    public Guid ReactorId { get; set; }
    public Guid ReactionTypeId { get; set; }
    public DateTime CreatedAt { get; set; }
    public ReactionType ReactionType { get; set; } = null!;
    public ChatUser Reactor { get; set; } = null!;
    public Post Post { get; set; } = null!;
}

public class CommentReaction : Entity<CommentReaction>
{
    public Guid CommentId { get; set; }
    public Guid ReactorId { get; set; }
    public Guid ReactionTypeId { get; set; }
    public DateTime CreatedAt { get; set; }
    public ReactionType ReactionType { get; set; } = null!;
    public ChatUser Reactor { get; set; } = null!;
    public Comment Comment { get; set; } = null!;
}

public enum Reactions
{
    None,
    Sticker,
    PostReaction
}