using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Abstractions.Services.Communications;
using Application.Services.ApplicationInfrastructure.Results;
using Domain.Models.Media;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Application.Requests.Commands.Chats;

public record UpdateChatCommand(
    Guid ChatId,
    Guid CurrentUserId,
    string? Name = null,
    IFormFile? Picture = null,
    List<Guid>? ParticipantIds = null,
    Guid? NewAdminId = null) : IRequest;

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
            RuleFor(x => x.ParticipantIds!.Count).LessThan(100).WithMessage("Too many participants");
        });
    }
}

public class UpdateChatCommandHandler(
    IChatsRepository chatsRepository,
    IChatUsersRepository chatUsersRepository,
    IFilesStorage filesStorage,
    IFilesValidator filesValidator,
    IChatNotificationService notificationService) : IRequestHandler<UpdateChatCommand>
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

        if (request.Picture != null)
        {
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

            await chatsRepository.AddPictureAsync(chatPicture, cancellationToken);
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

            var currentParticipantIds = chat.Users.Select(u => u.Id).ToHashSet();
            var newParticipantIds = newParticipants.Select(p => p.Id).ToHashSet();
            
            var addedParticipants = newParticipants.Where(p => !currentParticipantIds.Contains(p.Id)).ToList();
            
            var removedParticipants = chat.Users.Where(u => !newParticipantIds.Contains(u.Id)).ToList();

            chat.Users.Clear();
            foreach (var participant in newParticipants)
            {
                chat.Users.Add(participant);
            }
            
            var joinNotifications = addedParticipants.Select(p => 
                notificationService.NotifyUserJoinedChatAsync(chat.Id, p.Id, p.Username, chat.Name));
            
            var leaveNotifications = removedParticipants.Select(p => 
                notificationService.NotifyUserLeftChatAsync(chat.Id, p.Id, p.Username, chat.Name));
            
            chatsRepository.Update(chat);
            await chatsRepository.SaveChangesAsync(cancellationToken);
            
            await Task.WhenAll(joinNotifications.Concat(leaveNotifications));
        }

        if (request.NewAdminId.HasValue && request.NewAdminId != chat.AdminId)
        {
            if (chat.Users.All(u => u.Id != request.NewAdminId.Value))
            {
                return ResultsHelper.BadRequest("New admin must be a chat participant");
            }

            chat.AdminId = request.NewAdminId.Value;
        }

        if (request.ParticipantIds == null)
        {
            chatsRepository.Update(chat);
            await chatsRepository.SaveChangesAsync(cancellationToken);
        }

        return ResultsHelper.Ok(new { ChatId = chat.Id, Message = "Chat updated successfully" });
    }
} 