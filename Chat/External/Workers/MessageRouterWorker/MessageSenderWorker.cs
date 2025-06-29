using Application.Abstractions.Services.Communications;
using Application.Common.Helpers;

namespace MessageRouterWorker;

public class MessageSenderWorker(ILogger<MessageSenderWorker> logger, IMessageRouter messageRouter) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Worker starting at: {time}", DateHelper.GetCurrentDateTime());
        await messageRouter.StartAsync(cancellationToken);
        logger.LogInformation("Worker started at: {time}", DateHelper.GetCurrentDateTime());
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Worker stopping at: {time}", DateHelper.GetCurrentDateTime());
        await messageRouter.StopAsync(cancellationToken);
        logger.LogInformation("Worker stopped at: {time}", DateHelper.GetCurrentDateTime());
    }
}