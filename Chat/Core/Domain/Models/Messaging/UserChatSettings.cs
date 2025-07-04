using Domain.Models.Users;

namespace Domain.Models.Messaging;

public class UserChatSettings
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ChatId { get; set; }
    
    public bool IsMuted { get; set; }
    public bool IsDisabled { get; set; }
    
    public List<ChatUser> DisabledUsers { get; set; } = null!;
    public ChatUser User { get; set; } = null!;
    public Chat Chat { get; set; } = null!;
}