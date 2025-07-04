using Domain.Abstractions;
using Domain.Models.Blog;
using Domain.Models.Messaging;
using Domain.Models.Service;

namespace Domain.Models.Users;

public class ChatUser : IEntity
{
    public ChatUser(AspNetUser requestAspNetUser)
    {
        Id = requestAspNetUser.Id;
        Username = requestAspNetUser.UserName;
        AspNetUser = requestAspNetUser;
    }

    public Guid NotificationSettingsId { get; set; }
    public NotificationSettings NotificationSettings { get; set; } = null!;
    public Guid? CoverId { get; set; }
    public Guid? AvatarId { get; set; }
    public ChatUser() { }
    public Guid Id { get; set; }
    public AspNetUser AspNetUser { get; set; }
    public string Username { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? MiddleName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public List<Chat> Chats { get; set; } = [];
    public Attachment? Avatar { get; set; }
    public Attachment? Cover { get; set; }
    public UserPrivacySettings PrivacySettings { get; set; } = null!;

    public List<ChatUser> Friends { get; set; } = [];
    public List<UserChatSettings> ChatSettings { get; set; } = [];

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

public class UserPrivacySettings
{
    public Guid Id { get; set; }
    public Guid ChatUserId { get; set; }
    public ChatUser ChatUser { get; set; } = null!;
    
    public PrivacyLevel FriendsListVisibility { get; set; } = PrivacyLevel.Public;
    
    public PrivacyLevel CommentsPermission { get; set; } = PrivacyLevel.Public;
    
    public PrivacyLevel DirectMessagesPermission { get; set; } = PrivacyLevel.Public;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum PrivacyLevel
{
    /// <summary>
    /// Only me
    /// </summary>
    Private = 0,
    
    /// <summary>
    /// Only friends
    /// </summary>
    Friends = 1,
    
    /// <summary>
    /// For all
    /// </summary>
    Public = 2
}