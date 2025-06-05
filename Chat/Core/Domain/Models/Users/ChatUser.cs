using Domain.Abstractions;
using Domain.Models.Blog;
using Domain.Models.Messaging;

namespace Domain.Models;

public class ChatUser : IEntity
{
    public Guid Id { get; set; }
    public AspNetUser AspNetUser { get; set; } = null!;
    public string Username { get; set; } = null!;
    public DateTime LastSeen { get; set; }
    public List<Chat> Chats { get; set; } = [];
    public List<ChatUser> Friends { get; set; } = [];
    // Pending friend requests
    public List<ChatUser> FriendRequests { get; set; } = [];
    // Sent friend requests
    public List<ChatUser> WaitingFriendRequests { get; set; } = [];
    public List<Post> Posts { get; set; } = [];
    public List<ChatUser> BlockedUsers { get; set; } = [];

    public ChatUser CreateFrom(AspNetUser user)
    {
        return new ChatUser()
        {
            Id = user.Id,
            AspNetUser = user,
            Username = user.UserName
        };
    }
}

public class AspNetUser
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public ChatRole Role { get; set; } = null!;
}