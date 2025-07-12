using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using Domain.Models.Media;
using Domain.Models.Users;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Application.Requests.Commands.Chats;

public record UpdateChatCommand(
    Guid ChatId,
    Guid CurrentUserId,
    string? Name = null,
    IFormFile? Picture = null,
    Guid[]? ParticipantIds = null,
    Guid? NewAdminId = null,
    bool DeletePicture = false) : IRequest;

public class UpdateChatCommandValidator : AbstractValidator<UpdateChatCommand>
{
    public UpdateChatCommandValidator()
    {
        RuleFor(x => x.ChatId).NotEmpty().WithMessage("Chat ID is required");
        RuleFor(x => x.CurrentUserId).NotEmpty().WithMessage("Current user ID is required");
        
        When(x => !string.IsNullOrEmpty(x.Name), () =>
        {
            RuleFor(x => x.Name).MaximumLength(100).WithMessage("Chat name cannot exceed 100 characters");
        });
        
        When(x => x.ParticipantIds != null, () =>
        {
            RuleFor(x => x.ParticipantIds!.Length).LessThan(100).WithMessage("Too many participants");
        });
    }
}

public class UpdateChatCommandHandler(
    IChatsRepository chatsRepository,
    IChatUsersRepository chatUsersRepository,
    IFilesStorage filesStorage,
    IFilesValidator filesValidator) : IRequestHandler<UpdateChatCommand>
{
    public async Task<IOperationResult> HandleAsync(UpdateChatCommand request, CancellationToken cancellationToken = default)
    {
        var chat = await chatsRepository.GetByIdWithUsersAsync(request.ChatId, cancellationToken);
        if (chat == null)
        {
            return ResultsHelper.NotFound("Chat not found");
        }

        if (chat.AdminId != request.CurrentUserId)
        {
            return ResultsHelper.Forbidden("Only chat admin can update chat settings");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            chat.Name = request.Name.Trim();
        }

        if (request.DeletePicture)
        {
            if (chat.ChatPicture != null && !string.IsNullOrEmpty(chat.ChatPicture.Url))
            {
                await filesStorage.DeleteAsync(chat.ChatPicture.Url, cancellationToken);
            }
            chat.ChatPicture = null;
            chat.ChatPictureId = null;
        }
        else if (request.Picture != null)
        {
            if (chat.ChatPicture != null && !string.IsNullOrEmpty(chat.ChatPicture.Url))
            {
                await filesStorage.DeleteAsync(chat.ChatPicture.Url, cancellationToken);
            }

            using var memoryStream = new MemoryStream();
            await request.Picture.CopyToAsync(memoryStream, cancellationToken);

            if (!filesValidator.ValidateFile(memoryStream, request.Picture.FileName))
            {
                return ResultsHelper.BadRequest("Invalid image file");
            }

            var uploadResult = await filesStorage.UploadAsync(memoryStream, request.Picture.FileName, cancellationToken);
            if (!uploadResult.IsSuccess)
            {
                return ResultsHelper.BadRequest("Failed to upload image");
            }

            var chatPicture = new Picture
            {
                Id = Guid.NewGuid(),
                Url = uploadResult.GetValue<string>()
            };

            chat.ChatPicture = chatPicture;
            chat.ChatPictureId = chatPicture.Id;
        }

        if (request.ParticipantIds != null)
        {
            var participantIds = request.ParticipantIds.ToList();
            if (!participantIds.Contains(chat.AdminId))
            {
                participantIds.Add(chat.AdminId);
            }

            var newParticipants = await chatUsersRepository.GetByIdsAsync(participantIds, cancellationToken);
            if (newParticipants.Count != participantIds.Count)
            {
                return ResultsHelper.BadRequest("Some participants not found");
            }

            chat.Users.Clear();
            foreach (var participant in newParticipants)
            {
                chat.Users.Add(participant);
            }
        }

        if (request.NewAdminId.HasValue && request.NewAdminId != chat.AdminId)
        {
            if (chat.Users.All(u => u.Id != request.NewAdminId.Value))
            {
                return ResultsHelper.BadRequest("New admin must be a chat participant");
            }

            chat.AdminId = request.NewAdminId.Value;
        }

        chatsRepository.Update(chat);
        await chatsRepository.SaveChangesAsync(cancellationToken);

        return ResultsHelper.Ok(new { ChatId = chat.Id, Message = "Chat updated successfully" });
    }
} 