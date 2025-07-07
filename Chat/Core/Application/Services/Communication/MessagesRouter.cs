using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Services.Communications;
using Application.Abstractions.Services.Communications.Data;
using Application.Common.Helpers;
using Application.Common.Mappers;
using Application.Dtos.Messaging;
using Microsoft.Extensions.Logging;

namespace Application.Services.Communication;

public class MessagesRouter(
    IMessagesToRouteQueue messagesToRouteQueue,
    IChatsRepository chatsRepository,
    IUserConnectionsCache userConnections,
    IMessagesToSendQueue messagesToSendQueue,
    ILogger<MessagesRouter> logger) : IMessageRouter
{
    private bool isStopRequested;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Router starting at: {time}", DateHelper.GetCurrentDateTime());

        while (cancellationToken.IsCancellationRequested is false && isStopRequested is false)
        {
            var message = await messagesToRouteQueue.ReadAsync(cancellationToken);
            ThreadPool.UnsafeQueueUserWorkItem(new RoutingMessageHandle(message, chatsRepository, userConnections, messagesToSendQueue, logger), false);
        }

        logger.LogInformation("Router stopped at: {time}", DateHelper.GetCurrentDateTime());
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        isStopRequested = true;
        logger.LogInformation("Router stopping requested at: {time}", DateHelper.GetCurrentDateTime());

        return Task.CompletedTask;
    }
}

file class RoutingMessageHandle(
    MessageToRoute? message,
    IChatsRepository chatsRepository,
    IUserConnectionsCache userConnections,
    IMessagesToSendQueue messagesToSendQueue,
    ILogger<MessagesRouter> logger) : IThreadPoolWorkItem
{
    public async void Execute()
    {
        try
        {
            var userIdsFromChat = await chatsRepository.GetUserIdsFromChatNoTrackingAsync(message.ChatId);
            userIdsFromChat = userIdsFromChat.Where(id => id.ToString().Equals(message.UserId) is false).ToArray();
            
            var connections = await userConnections.GetConnectionsForUsersAsync(userIdsFromChat);

            var messageToSend = Messages.MessageToSend(message);
            // messageToSend.ConnectionIds = connections;

            await messagesToSendQueue.WriteAsync(messageToSend, CancellationToken.None);

            logger.LogInformation("Message routed at: {time}, message: {MessageId}", DateHelper.GetCurrentDateTime(), message.MessageId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while routing message {MessageId}, time: {time}", message.MessageId, DateHelper.GetCurrentDateTime());
        }
        finally
        {
            logger.LogInformation("Message routed at: {time}, message: {MessageId}", DateHelper.GetCurrentDateTime(), message.MessageId);
        }
    }
}