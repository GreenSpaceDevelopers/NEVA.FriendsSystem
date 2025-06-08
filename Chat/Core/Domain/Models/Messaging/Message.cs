using Domain.Abstractions;
using Domain.Models.Users;

namespace Domain.Models.Messaging;

public class Message : Entity<Message>
{
    public Guid ChatId { get; set; }
    public Guid SenderId { get; set; }
    public Guid AttachmentId { get; set; } = Guid.Empty;
    public string? Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public List<Message> Replies { get; set; } = [];
    public Chat Chat { get; set; } = null!;
    public ChatUser Sender { get; set; } = null!;
    public Attachment? Attachment { get; set; }
    public List<Reaction> Reactions { get; set; } = [];
}