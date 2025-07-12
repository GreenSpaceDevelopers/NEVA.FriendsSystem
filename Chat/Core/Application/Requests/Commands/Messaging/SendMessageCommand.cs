using Application.Abstractions.Persistence.Repositories.Media;
using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Abstractions.Services.Communications;
using Application.Services.ApplicationInfrastructure.Results;
using Domain.Models.Messaging;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Application.Requests.Commands.Messaging;

public record SendMessageCommand(Guid ChatId, Guid SenderId, string Content, IFormFileCollection? Attachments = null) : IRequest;

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.ChatId).NotEmpty().WithMessage("ID чата обязателен");
        RuleFor(x => x.SenderId).NotEmpty().WithMessage("ID отправителя обязателен");
        
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Content) || x.Attachments is { Count: > 0 })
            .WithMessage("Сообщение должно содержать либо текст, либо вложение");
            
        RuleFor(x => x.Content)
            .MaximumLength(2000).WithMessage("Сообщение не может быть длиннее 2000 символов")
            .When(x => !string.IsNullOrWhiteSpace(x.Content));

        RuleFor(x => x.Attachments)
            .Must(files => files is not { Count: > 10 }).WithMessage("Maximum 10 files allowed.");
    }
}

public class SendMessageCommandHandler(
    IChatsRepository chatsRepository,
    IMessagesRepository messagesRepository,
    IChatNotificationService notificationService,
    IFilesStorage filesStorage,
    IFilesValidator filesValidator,
    IAttachmentsRepository attachmentsRepository) : IRequestHandler<SendMessageCommand>
{
    public async Task<IOperationResult> HandleAsync(SendMessageCommand request, CancellationToken cancellationToken = default)
    {
        var chat = await chatsRepository.GetByIdWithUsersAsync(request.ChatId, cancellationToken);
        if (chat is null)
        {
            return ResultsHelper.NotFound("Чат не найден");
        }

        if (chat.Users.All(u => u.Id != request.SenderId))
        {
            return ResultsHelper.Forbidden("Вы не являетесь участником данного чата");
        }

        var attachmentsList = new List<Attachment>();

        if (request.Attachments is not null && request.Attachments.Count > 0)
        {
            if (request.Attachments.Count > 5) // Максимум 5 файлов для сообщения
            {
                return ResultsHelper.BadRequest("Maximum 5 files allowed");
            }

            foreach (var file in request.Attachments)
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream, cancellationToken);

                if (!filesValidator.ValidateFile(memoryStream, file.FileName))
                {
                    return ResultsHelper.BadRequest($"Недопустимый файл: {file.FileName}");
                }

                var uploadResult = await filesStorage.UploadAsync(memoryStream, file.FileName, cancellationToken);
                if (!uploadResult.IsSuccess)
                {
                    return ResultsHelper.BadRequest($"Ошибка загрузки файла: {file.FileName}");
                }

                var attachmentType = await attachmentsRepository.GetAttachmentTypeAsync(
                    GetAttachmentTypeFromFileName(file.FileName), 
                    cancellationToken);

                var attachment = new Attachment
                {
                    Id = Guid.NewGuid(),
                    Url = uploadResult.GetValue<string>(),
                    Type = attachmentType,
                    TypeId = attachmentType.Id
                };

                await attachmentsRepository.AddAsync(attachment, cancellationToken);
                attachmentsList.Add(attachment);
            }

            await attachmentsRepository.SaveChangesAsync(cancellationToken);
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ChatId = request.ChatId,
            SenderId = request.SenderId,
            Content = request.Content,
            Attachments = attachmentsList,
            CreatedAt = DateTime.UtcNow
        };

        await messagesRepository.AddAsync(message, cancellationToken);
        await messagesRepository.SaveChangesAsync(cancellationToken);

        chat.LastMessageDate = message.CreatedAt;
        chatsRepository.Update(chat);
        await chatsRepository.SaveChangesAsync(cancellationToken);

        var sender = chat.Users.FirstOrDefault(u => u.Id == request.SenderId);
        var senderName = sender?.Username ?? "Unknown";

        var recipientUserIds = chat.Users
            .Where(u => u.Id != request.SenderId)
            .Select(u => u.Id)
            .ToList();

        var chatName = GetChatDisplayName(chat, request.SenderId);

        var notificationContent = GetNotificationContent(request.Content, attachmentsList);

        if (attachmentsList.Count > 0)
        {
            await notificationService.NotifyNewMessageWithAttachmentAsync(
                request.ChatId, 
                request.SenderId, 
                senderName, 
                notificationContent, 
                attachmentsList.First().Url,
                message.CreatedAt);

            await notificationService.NotifyUsersAboutNewMessageWithAttachmentAsync(
                recipientUserIds,
                request.ChatId,
                request.SenderId,
                senderName,
                notificationContent,
                attachmentsList.First().Url,
                message.CreatedAt,
                chatName);
        }
        else
        {
            await notificationService.NotifyNewMessageAsync(
                request.ChatId, 
                request.SenderId, 
                senderName, 
                notificationContent, 
                message.CreatedAt);

            await notificationService.NotifyUsersAboutNewMessageAsync(
                recipientUserIds,
                request.ChatId,
                request.SenderId,
                senderName,
                notificationContent,
                message.CreatedAt,
                chatName);
        }

        return ResultsHelper.Ok(new { MessageId = message.Id, SentAt = message.CreatedAt });
    }

    private static AttachmentTypes GetAttachmentTypeFromFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" => AttachmentTypes.Image,
            ".mp4" or ".avi" or ".mov" or ".wmv" or ".flv" or ".webm" => AttachmentTypes.Video,
            ".mp3" or ".wav" or ".flac" or ".aac" or ".ogg" => AttachmentTypes.Audio,
            _ => AttachmentTypes.File
        };
    }

    private static string GetChatDisplayName(Chat chat, Guid currentUserId)
    {
        if (!string.IsNullOrEmpty(chat.Name))
        {
            return chat.Name;
        }

        if (chat.Users.Count == 2)
        {
            var interlocutor = chat.Users.FirstOrDefault(u => u.Id != currentUserId);
            return interlocutor?.Username ?? "Неизвестный пользователь";
        }

        var participants = chat.Users
            .Where(u => u.Id != currentUserId)
            .Take(3)
            .Select(u => u.Username)
            .ToList();

        if (participants.Count == 0)
        {
            return "Групповой чат";
        }

        var chatName = string.Join(", ", participants);
        if (chat.Users.Count > 4)
        {
            chatName += $" и ещё {chat.Users.Count - 4}";
        }

        return chatName;
    }

    private static string GetNotificationContent(string content, List<Attachment> attachments)
    {
        if (!string.IsNullOrWhiteSpace(content))
        {
            return content;
        }

        if (attachments.Count <= 0)
        {
            return "📝 Сообщение";
        }
        
        var firstAttachment = attachments.First();
        var baseContent = GetAttachmentTypeFromFileName(firstAttachment.Url) switch
        {
            AttachmentTypes.Image => "📷 Фото",
            AttachmentTypes.Video => "🎥 Видео", 
            AttachmentTypes.Audio => "🎵 Аудио",
            AttachmentTypes.Sticker => "✨ Стикер",
            AttachmentTypes.File or _ => "📎 Файл"
        };

        return attachments.Count > 1 ? $"{baseContent} (+{attachments.Count - 1})" : baseContent;

    }
} 