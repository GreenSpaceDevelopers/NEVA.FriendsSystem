namespace Application.Dtos.Responses.Friends;

public record FriendRequestsDto(Guid sender, Guid receiver, int count);