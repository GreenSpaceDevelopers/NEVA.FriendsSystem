using Domain.Abstractions;
using Domain.Models.Media;
using Domain.Models.Service;
using Domain.Models.Users;

namespace Domain.Models.Messaging;

public class Chat : Entity<Chat>
{
    public string Name { get; set; } = null!;
    public Guid ChatPictureId { get; set; } = Guid.Empty;
    public Guid AdminId { get; set; }
    public Guid RelatedEventId { get; set; }
    public DateTime LastMessageDate { get; set; }

    public Event? RelatedEvent { get; set; }
    public Picture? ChatPicture { get; set; }
    public ChatUser? Admin { get; set; }
    public List<ChatUser> Users { get; set; } = [];
    public List<Message> Messages { get; set; } = [];
}