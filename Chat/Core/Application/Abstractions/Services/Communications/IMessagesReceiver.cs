namespace Application.Abstractions.Services.Communications;

public interface IMessagesReceiver
{
    public Task StartAsync(CancellationToken cancellationToken = default);
    public Task StopAsync(CancellationToken cancellationToken = default);
}