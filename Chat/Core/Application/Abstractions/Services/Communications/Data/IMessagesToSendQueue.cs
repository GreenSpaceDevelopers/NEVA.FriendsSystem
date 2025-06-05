namespace Application.Abstractions.Services.Communications.Data;

public interface IMessagesToSendQueue
{
    public Task WriteAsync(object messageToSend, CancellationToken token);
}