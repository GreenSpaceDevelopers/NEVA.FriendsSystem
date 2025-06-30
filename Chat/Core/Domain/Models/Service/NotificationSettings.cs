using Domain.Abstractions;
using Domain.Models.Users;

namespace Domain.Models.Service;

public class NotificationSettings : IEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ChatUser User { get; set; } = null!;
    
    public bool IsEnabled { get; set; }
    public bool IsTelegramEnabled { get; set; }
    public bool IsEmailEnabled { get; set; }
    public bool IsPushEnabled { get; set; }
    
    public bool NewTournamentPosts { get; set; }
    public bool TournamentUpdates { get; set; }
    public bool TournamentTeamInvites { get; set; }
    
    public bool TeamChanged { get; set; }
    public bool TeamInvites { get; set; }
    public bool TournamentInvites { get; set; }
    
    public bool AdminRoleAssigned { get; set; }
    public bool TournamentLobbyInvites { get; set; }
    public bool NewTournamentStep { get; set; }
    public bool TournamentStarted { get; set; }
    
    public bool NewFriendRequest { get; set; }
    public bool NewMessage { get; set; }
    public bool NewPostComment { get; set; }
    public bool NewCommentReply { get; set; }
    public bool NewCommentReaction { get; set; }
}