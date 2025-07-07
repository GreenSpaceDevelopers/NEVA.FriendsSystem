namespace Application.Abstractions.Services.Communications;

public interface IChatHub
{
    Task JoinChat(string chatId);
    Task LeaveChat(string chatId);
    Task SendMessage(string chatId, string message);
} 