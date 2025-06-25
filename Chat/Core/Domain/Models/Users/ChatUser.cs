using Domain.Abstractions;
using Domain.Models.Blog;
using Domain.Models.Messaging;

namespace Domain.Models.Users;

public class ChatUser(AspNetUser requestAspNetUser) : IEntity
{
    public Guid Id { get; set; } = requestAspNetUser.Id;
    public AspNetUser AspNetUser { get; set; } = requestAspNetUser;
    public string Username { get; set; } = requestAspNetUser.UserName;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public List<Chat> Chats { get; set; } = [];
    public Attachment? Avatar { get; set; }
    public Attachment? Cover { get; set; }
    public PrivacySetting PrivacySetting { get; set; } = null!;

    public List<ChatUser> Friends { get; set; } = [];

    // Pending friend requests
    public List<ChatUser> FriendRequests { get; set; } = [];

    // Sent friend requests
    public List<ChatUser> WaitingFriendRequests { get; set; } = [];
    public List<Post> Posts { get; set; } = [];
    public List<ChatUser> BlockedUsers { get; set; } = [];

    public ChatUser CreateFrom(AspNetUser user, int role)
    {
        return new ChatUser(AspNetUser);
    }
}

public class AspNetUser
{
    public Guid Id { get; set; }
    public Guid ChatUserId { get; set; }
    public Guid RoleId { get; set; }
    public string Email { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public ChatRole Role { get; set; } = null!;
}

public class PrivacySetting
{
    public int Id { get; set; }
    public Guid ChatUserId { get; set; }
    public string SettingName { get; set; } = null!;
}

public enum PrivacySettingsEnums
{
    Private = 0,
    Friends = 1,
    Public = 2,
}