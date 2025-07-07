using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Abstractions.Services.Communications;
using Application.Abstractions.Services.Communications.Data;
using Application.Services.ApplicationInfrastructure.Results;
using Application.Dtos.Messaging;
using Domain.Models.Messaging;
using FluentValidation;

namespace Application.Requests.Commands.Messaging;

public record SendMessageCommand(Guid ChatId, Guid SenderId, string Content) : IRequest;

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.ChatId).NotEmpty().WithMessage("ID чата обязателен");
        RuleFor(x => x.SenderId).NotEmpty().WithMessage("ID отправителя обязателен");
        RuleFor(x => x.Content).NotEmpty().WithMessage("Содержимое сообщения обязательно")
            .MaximumLength(2000).WithMessage("Сообщение не может быть длиннее 2000 символов");
    }
}

public class SendMessageCommandHandler(
    IChatsRepository chatsRepository,
    IMessagesRepository messagesRepository,
    IMessagesToRouteQueue messagesToRouteQueue,
    IChatNotificationService notificationService) : IRequestHandler<SendMessageCommand>
{
    public async Task<IOperationResult> HandleAsync(SendMessageCommand request, CancellationToken cancellationToken = default)
    {
        var chat = await chatsRepository.GetByIdAsync(request.ChatId, cancellationToken);
        if (chat is null)
        {
            return ResultsHelper.NotFound("Чат не найден");
        }

        if (chat.Users.All(u => u.Id != request.SenderId))
        {
            return ResultsHelper.Forbidden("Вы не являетесь участником данного чата");
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ChatId = request.ChatId,
            SenderId = request.SenderId,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow
        };

        await messagesRepository.AddAsync(message, cancellationToken);
        await messagesRepository.SaveChangesAsync(cancellationToken);

        chat.LastMessageDate = message.CreatedAt;
        chatsRepository.Update(chat);
        await chatsRepository.SaveChangesAsync(cancellationToken);

        var sender = chat.Users.FirstOrDefault(u => u.Id == request.SenderId);
        var senderName = sender?.Username ?? "Unknown";

        await notificationService.NotifyNewMessageAsync(
            request.ChatId, 
            request.SenderId, 
            senderName, 
            request.Content, 
            message.CreatedAt);

        var messageToRoute = new MessageToRoute(
            ChatId: request.ChatId.ToString(),
            UserId: request.SenderId.ToString(),
            MessageId: message.Id.ToString()
        );

        await messagesToRouteQueue.WriteAsync(messageToRoute, cancellationToken);

        return ResultsHelper.Ok(new { MessageId = message.Id, SentAt = message.CreatedAt });
    }
} 