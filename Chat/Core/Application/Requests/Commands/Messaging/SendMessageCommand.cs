using Application.Abstractions.Persistence.Repositories.Media;
using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Abstractions.Services.Communications.Data;
using Application.Dtos.Messaging;
using Application.Services.ApplicationInfrastructure.Results;
using Application.Dtos.Requests.Shared;
using Domain.Models.Messaging;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Application.Requests.Commands.Messaging;

public record SendMessageCommand(
    Guid ChatId,
    Guid SenderId,
    string? Content,
    IFormFile? Attachment) : IRequest;

public class SendMessageCommandHandler(
    IChatsRepository chatsRepository,
    IMessagesRepository messagesRepository,
    IAttachmentsRepository attachmentsRepository,
    IFilesStorage filesStorage,
    IFilesValidator filesValidator,
    IMessagesToRouteQueue messagesToRouteQueue,
    IChatUsersRepository chatUsersRepository) : IRequestHandler<SendMessageCommand>
{
    public async Task<IOperationResult> HandleAsync(SendMessageCommand request, CancellationToken cancellationToken = default)
    {
        var chat = await chatsRepository.GetByIdAsync(request.ChatId, cancellationToken);
        if (chat is null)
        {
            return ResultsHelper.NotFound("Chat not found");
        }

        if (chat.Users.All(u => u.Id != request.SenderId))
        {
            return ResultsHelper.Forbidden("You are not a member of this chat");
        }
        
        Attachment? attachment = null;
        if (request.Attachment is not null)
        {
            using var stream = new MemoryStream();
            await request.Attachment.CopyToAsync(stream, cancellationToken);

            if (!filesValidator.ValidateFile(stream, request.Attachment.FileName))
            {
                return ResultsHelper.BadRequest("Invalid attachment file");
            }

            var uploadResult = await filesStorage.UploadAsync(stream, request.Attachment.FileName, cancellationToken);
            if (!uploadResult.IsSuccess)
            {
                return ResultsHelper.BadRequest(uploadResult.GetValue<string>());
            }

            var attachmentType = await attachmentsRepository.GetAttachmentTypeAsync(AttachmentTypes.Image, cancellationToken);
            attachment = new Attachment
            {
                Id = Guid.NewGuid(),
                Url = uploadResult.GetValue<string>(),
                Type = attachmentType,
                TypeId = attachmentType.Id
            };
            await attachmentsRepository.AddAsync(attachment, cancellationToken);
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ChatId = request.ChatId,
            SenderId = request.SenderId,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow,
            Attachment = attachment,
            AttachmentId = attachment?.Id ?? Guid.Empty,
        };

        await messagesRepository.AddAsync(message, cancellationToken);
        chat.LastMessageDate = DateTime.UtcNow;
        await messagesRepository.SaveChangesAsync(cancellationToken);

        var messageToRoute = new MessageToRoute(
            ConnectionId: string.Empty,
            UserId: request.SenderId.ToString(),
            ChatId: request.ChatId.ToString(),
            MessageId: message.Id.ToString());
        await messagesToRouteQueue.WriteAsync(messageToRoute, cancellationToken);

        return ResultsHelper.Created(message.Id);
    }
}

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.ChatId).NotEmpty();
        RuleFor(x => x.SenderId).NotEmpty();
        RuleFor(x => x.Content)
            .MaximumLength(4000)
            .When(x => x.Content is not null);
    }
} 