using Application.Abstractions.Services.Communications;
using Application.Common.Helpers;

namespace ConnectionHolderWorker;

public class ConnectionHolderWorker(ILogger<ConnectionHolderWorker> logger, IConnectionHolder connectionHolder) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Worker starting at: {time}", DateHelper.GetCurrentDateTime());
        await connectionHolder.StartAsync(cancellationToken);
        logger.LogInformation("Worker started at: {time}", DateHelper.GetCurrentDateTime());
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Worker stopping at: {time}", DateHelper.GetCurrentDateTime());
        await connectionHolder.StopAsync(cancellationToken);
        logger.LogInformation("Worker stopped at: {time}", DateHelper.GetCurrentDateTime());
    }
}