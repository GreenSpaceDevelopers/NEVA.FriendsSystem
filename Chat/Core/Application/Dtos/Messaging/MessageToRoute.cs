namespace Application.Dtos.Messaging;

public record MessageToRoute(string Text = "",string ConnectionId = "", string UserId = "", string ChatId = "", string MessageId = "", Status Status = Status.Success);

public enum Status
{
    Unverified,
    Unauthorized,
    Success,
    ChatNotFound
}