using Application.Abstractions.Services.Communications;
using Application.Common.Helpers;

namespace MessageValidatorWorker;

public class MessageReceiverWorker(ILogger<MessageReceiverWorker> logger, IMessagesReceiver messagesReceiver) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Worker starting at: {time}", DateHelper.GetCurrentDateTime());
        await messagesReceiver.StartAsync(cancellationToken);
        logger.LogInformation("Worker started at: {time}", DateHelper.GetCurrentDateTime());
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Worker stopping at: {time}", DateHelper.GetCurrentDateTime());
        await messagesReceiver.StopAsync(cancellationToken);
        logger.LogInformation("Worker stopped at: {time}", DateHelper.GetCurrentDateTime());
    }
}