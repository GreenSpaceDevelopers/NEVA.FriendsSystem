namespace Application.Abstractions.Services.Communications;

public interface IConnectionHolder
{
    public Task StartAsync(CancellationToken cancellationToken);
    public Task StopAsync(CancellationToken cancellationToken);
}