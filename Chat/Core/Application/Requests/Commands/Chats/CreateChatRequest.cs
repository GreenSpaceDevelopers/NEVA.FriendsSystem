using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Persistence.Repositories.Media;
using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using Domain.Models.Messaging;
using Domain.Models.Users;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Application.Abstractions.Services.ApplicationInfrastructure.Data; // For IFilesStorage / Validator

namespace Application.Requests.Commands.Chats;

public record CreateChatRequest(
    Guid CurrentUserId,
    Guid[] UserIds,
    string? ChatName = null,
    IFormFile? ChatPicture = null) : IRequest;

public class CreateChatRequestValidator : AbstractValidator<CreateChatRequest>
{
    public CreateChatRequestValidator()
    {
        RuleFor(x => x.CurrentUserId).NotEmpty();
        RuleFor(x => x.UserIds).NotNull();
        RuleFor(x => x.UserIds.Length).Must(c => c < 100);
        When(x => x.ChatName is not null, () =>
        {
            RuleFor(x => x.ChatName).MaximumLength(100);
        });
    }
}

public class CreateChatRequestHandler(
    IChatUsersRepository chatUsersRepository,
    IChatsRepository chatsRepository,
    IFilesStorage filesStorage,
    IFilesValidator filesValidator,
    IAttachmentsRepository attachmentsRepository) : IRequestHandler<CreateChatRequest>
{
    public async Task<IOperationResult> HandleAsync(CreateChatRequest request, CancellationToken cancellationToken = default)
    {
        var chatUsers = new List<ChatUser>();

        foreach (var userId in request.UserIds)
        {
            var user = await chatUsersRepository.GetByIdAsync(userId, cancellationToken);

            if (user is not null)
            {
                chatUsers.Add(user);
            }
        }

        var currentUser = await chatUsersRepository.GetByIdAsync(request.CurrentUserId, cancellationToken);
        if (currentUser is not null && chatUsers.All(u => u.Id != currentUser.Id))
        {
            chatUsers.Add(currentUser);
        }
        
        if (chatUsers.Count == 0)
        {
            return ResultsHelper.NotFound("No valid users found to create a chat.");
        }

        if (chatUsers.Count == 2)
        {
            var user1Id = chatUsers[0].Id;
            var user2Id = chatUsers[1].Id;
            
            var existingChatInfo = await chatUsersRepository.GetChatInfoBetweenUsersAsync(user1Id, user2Id, cancellationToken);
            if (existingChatInfo?.ChatId is not null)
            {
                return ResultsHelper.Conflict("A private chat between these users already exists.");
            }
        }

        var adminId = request.CurrentUserId;

        string chatName;
        if (!string.IsNullOrWhiteSpace(request.ChatName))
        {
            chatName = request.ChatName.Trim();
        }
        else if (chatUsers.Count == 2)
        {
            var interlocutor = chatUsers.First(u => u.Id != request.CurrentUserId);
            chatName = interlocutor.Username;
        }
        else
        {
            chatName = string.Join(", ", chatUsers.Select(u => u.Username));
        }

        Attachment? chatPicture = null;

        if (request.ChatPicture is not null)
        {
            using var memoryStream = new MemoryStream();
            await request.ChatPicture.CopyToAsync(memoryStream, cancellationToken);

            if (!filesValidator.ValidateFile(memoryStream, request.ChatPicture.FileName))
            {
                return ResultsHelper.BadRequest("Недопустимый файл изображения");
            }

            var uploadResult = await filesStorage.UploadAsync(memoryStream, request.ChatPicture.FileName, cancellationToken);
            if (!uploadResult.IsSuccess)
            {
                return ResultsHelper.BadRequest("Ошибка загрузки изображения");
            }

            var fileResult = uploadResult.GetValue<FileUploadResult>();
            var imageType = await attachmentsRepository.GetAttachmentTypeAsync(AttachmentTypes.Image, cancellationToken);

            chatPicture = new Attachment
            {
                Id = Guid.NewGuid(),
                Url = fileResult.Url,
                FileId = fileResult.FileId,
                TypeId = imageType.Id,
                Type = imageType
            };
        }

        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Name = chatName,
            AdminId = adminId,
            Users = chatUsers.ToList(),
            ChatPicture = chatPicture,
            ChatPictureId = chatPicture?.Id
        };
        
        await chatsRepository.AddAsync(chat, cancellationToken);
        await chatsRepository.SaveChangesAsync(cancellationToken);
        
        return ResultsHelper.Created(chat.Id);
    }
}