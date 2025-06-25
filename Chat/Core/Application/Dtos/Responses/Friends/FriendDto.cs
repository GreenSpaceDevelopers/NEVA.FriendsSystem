namespace Application.Dtos.Responses.Friends;

public record FriendDto(
    Guid Id,
    string Username,
    string? AvatarUrl,
    DateTime LastSeen
); 