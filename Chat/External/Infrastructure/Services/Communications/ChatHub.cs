using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Requests.Commands.Messaging;

namespace Infrastructure.Services.Communications;

// [Authorize] // Временно отключаем авторизацию для тестирования
public class ChatHub(ISender sender) : Hub
{
    public async Task JoinChat(string chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Chat_{chatId}");
        await Clients.Group($"Chat_{chatId}").SendAsync("UserJoined", Context.User?.Identity?.Name);
    }

    public async Task LeaveChat(string chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Chat_{chatId}");
        await Clients.Group($"Chat_{chatId}").SendAsync("UserLeft", Context.User?.Identity?.Name);
    }

    public async Task SendMessage(string chatId, string message)
    {
        // Для тестирования используем фиксированный UserId
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        var command = new SendMessageCommand(Guid.Parse(chatId), userId, message);
        var result = await sender.SendAsync(command);

        if (result.IsSuccess)
        {
            await Clients.Group($"Chat_{chatId}").SendAsync("ReceiveMessage", new
            {
                MessageId = result.ObjectData,
                ChatId = chatId,
                SenderId = userId,
                Content = message,
                SentAt = DateTime.UtcNow
            });
        }
    }

    public override async Task OnConnectedAsync()
    {
        // Для тестирования используем фиксированный UserId
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Для тестирования используем фиксированный UserId
        var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
        await base.OnDisconnectedAsync(exception);
    }
} 