using Domain.Abstractions;
using Domain.Models.Users;

namespace Domain.Models.Messaging;

public class Reaction : Entity<Message>
{
    public Guid MessageId { get; set; }
    public Guid UserId { get; set; }
    
    public ReactionType Type { get; set; } = null!;
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

public enum Reactions
{
    None,
    Sticker
}