using Application.Abstractions.Services.Auth;
using Application.Abstractions.Services.Communications;
using Application.Abstractions.Services.Communications.Data;
using Application.Common.Helpers;
using Application.Common.Mappers;
using Application.Dtos.Messaging;
using Microsoft.Extensions.Logging;

namespace Application.Services.Communication;

public class MessagesesReceiver(
    IRawMessagesQueue rawMessagesQueue,
    ITokenValidator tokenValidator,
    ISigningService signingService,
    IUserConnectionsCache userConnections,
    IMessagesToRouteQueue messagesToRouteQueue,
    IMessagesToProcessQueue messagesToProcessQueue,
    ILogger<MessagesesReceiver> logger) : IMessagesReceiver
{
    private bool isStopRequested;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Receiver starting at: {time}", DateHelper.GetCurrentDateTime());

        while (cancellationToken.IsCancellationRequested is false && isStopRequested is false)
        {
            var message = await rawMessagesQueue.ReadAsync(cancellationToken);
            logger.LogInformation("Message received at: {time}", DateHelper.GetCurrentDateTime());
            ThreadPool.UnsafeQueueUserWorkItem(new ReceivedMessageHandle(message, tokenValidator, signingService, userConnections, messagesToRouteQueue, messagesToProcessQueue, logger), false);
        }

        logger.LogInformation("Receiver stopped at: {time}", DateHelper.GetCurrentDateTime());
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Receiver stopping requested at: {time}", DateHelper.GetCurrentDateTime());
        isStopRequested = true;
        return Task.CompletedTask;
    }
}

file class ReceivedMessageHandle(
    RawMessage? message,
    ITokenValidator tokenValidator,
    ISigningService signingService,
    IUserConnectionsCache userConnections,
    IMessagesToRouteQueue messagesToRouteQueue,
    IMessagesToProcessQueue messagesToProcessQueue,
    ILogger<MessagesesReceiver> logger) : IThreadPoolWorkItem
{
    public async void Execute()
    {
        try
        {
            if (signingService.Verify(message, message.Hash) is false && !string.IsNullOrWhiteSpace(message.Hash))
            {
                var unverifiedMessageResponse = message.Unverified();
                await messagesToRouteQueue.WriteAsync(unverifiedMessageResponse, CancellationToken.None);
                
                return;
            }

            if (await tokenValidator.ValidateToken(message.AccessToken) is false)
            {
                var unauthorizedMessageResponse = message.Unauthorized();
                await messagesToRouteQueue.WriteAsync(unauthorizedMessageResponse, CancellationToken.None);
                
                return;
            }

            var userId = await tokenValidator.GetUserIdFromToken(message.AccessToken);

            if (message.MessageType == RequestType.ConnectionRequest)
            {
                await userConnections.AddOrUpdateAsync(userId, message.ConnectionId!);
                return;
            }

            var messageToProcess = new MessageToProcess(userId, message.MessageId, message.Type, message.Message, message.StickerId, message.ReactionId, message.ChatId!);
            await messagesToProcessQueue.WriteAsync(messageToProcess, CancellationToken.None);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to verify message - {MessageId} at: {time}", message.MessageId, DateHelper.GetCurrentDateTime());
        }
        finally
        {
            logger.LogInformation("Message - {MessageId} validated at: {time},", message.MessageId, DateHelper.GetCurrentDateTime());
        }
    }
}