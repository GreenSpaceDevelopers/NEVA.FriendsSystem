using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.Communications;
using Application.Abstractions.Services.Communications.Data;
using Application.Common.Helpers;
using Application.Dtos.Messaging;
using Domain.Models.Messaging;
using Microsoft.Extensions.Logging;

namespace Application.Services.Communication;

public class MessageHandler(IMessagesToProcessQueue messagesToProcessQueue, IChatsRepository chatsRepository, ILogger<MessageHandler> logger,
    IMessagesToRouteQueue messagesToSendQueue, IChatUsersRepository chatUsersRepository) : IMessageHandler
{
    private bool isStopRequested;
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        while (!isStopRequested && cancellationToken.IsCancellationRequested is false)
        {
            var message = await messagesToProcessQueue.ReadAsync(cancellationToken);
            ThreadPool.UnsafeQueueUserWorkItem(new ProcessMessageHandle(message, chatsRepository, logger, messagesToSendQueue, chatUsersRepository), false);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        isStopRequested = true;
        return Task.CompletedTask;
    }
}

file class ProcessMessageHandle(MessageToProcess? message, IChatsRepository chatsRepository, ILogger<MessageHandler> logger,
    IMessagesToRouteQueue messagesToSendQueue, IChatUsersRepository chatUsersRepository) : IThreadPoolWorkItem
{
    public async void Execute()
    {
        try
        {
            if (message.Message is null)
            {
                var messageToSend = new MessageToRoute
                {
                    Text = "Message can not be empty",
                    MessageId = message.messageMessageId.ToString(),
                    UserId = message.UserId,
                    Status = Status.Unverified,
                };
                
                await messagesToSendQueue.WriteAsync(messageToSend, CancellationToken.None);
                return;
            }
            
            var chat = await chatsRepository.GetByIdAsync(Guid.Parse(message.ChatId));
            
            if (chat is null)
            {
                var messageToSend = new MessageToRoute
                {
                    Text = "Chat not found",
                    MessageId = message.messageMessageId.ToString(),
                    UserId = message.UserId,
                    Status = Status.Success,
                };
                
                await messagesToSendQueue.WriteAsync(messageToSend, CancellationToken.None);
                return;
            }
            
            var sender = await chatUsersRepository.GetByIdAsync(Guid.Parse(message.UserId));
            
            if (sender is null)
            {
                var messageToSend = new MessageToRoute
                {
                    Text = "Sender not found",
                    MessageId = message.messageMessageId.ToString(),
                    UserId = message.UserId,
                    Status = Status.Unauthorized,
                };
                
                await messagesToSendQueue.WriteAsync(messageToSend, CancellationToken.None);
                return;
            }
            
            sender.LastSeen = DateHelper.GetCurrentDateTime();
            chatUsersRepository.Update(sender);
            
            if (chat.Users.All(x => x.Id != Guid.Parse(message.UserId)) || chat.Admin?.Id != Guid.Parse(message.UserId))
            {
                var messageToSend = new MessageToRoute
                {
                    Text = "Sender not in chat",
                    MessageId = message.messageMessageId.ToString(),
                    UserId = message.UserId,
                    Status = Status.Unauthorized,
                };
                
                await messagesToSendQueue.WriteAsync(messageToSend, CancellationToken.None);
                return;
            }
            
            chat.Messages.Add(new Message
            {
                Id = message.messageMessageId,
                SenderId = Guid.Parse(message.UserId),
                Content = message.Message,
                CreatedAt = DateHelper.GetCurrentDateTime(),
            });
            
            chatsRepository.Update(chat);
            
            await chatsRepository.SaveChangesAsync();
            
            var messageToRoute = new MessageToRoute
            {
                Text = message.Message!,
                MessageId = message.messageMessageId.ToString(),
                UserId = message.UserId,
                Status = Status.Success,
            };
            
            await messagesToSendQueue.WriteAsync(messageToRoute, CancellationToken.None);
        }
        catch (Exception e)
        {
            logger .LogError(e, "Error while processing message {MessageId}, time: {time}", message.messageMessageId, DateHelper.GetCurrentDateTime());
        }
    }
}