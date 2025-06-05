namespace Application.Abstractions.Services.Communications;

public interface IMessageHandler
{
    public Task StartAsync(CancellationToken cancellationToken = default);
    public Task StopAsync(CancellationToken cancellationToken = default);
}