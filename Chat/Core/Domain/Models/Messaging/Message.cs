using Domain.Abstractions;
using Domain.Models.Users;

namespace Domain.Models.Messaging;

public class Message : Entity<Message>
{
    public Guid ChatId { get; set; }
    public Guid SenderId { get; set; }
    public string? Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Navigation
    public List<Message> Replies { get; set; } = [];
    public Chat Chat { get; set; } = null!;
    public ChatUser Sender { get; set; } = null!;
    public List<Attachment> Attachments { get; set; } = [];
    public List<MessageReaction> Reactions { get; set; } = [];
}