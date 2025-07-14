using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Abstractions.Services.Communications;
using Application.Services.ApplicationInfrastructure.Results;
using FluentValidation;

namespace Application.Requests.Commands.Chats;

public record LeaveChatCommand(Guid ChatId, Guid UserId) : IRequest;

public class LeaveChatCommandValidator : AbstractValidator<LeaveChatCommand>
{
    public LeaveChatCommandValidator()
    {
        RuleFor(x => x.ChatId).NotEmpty().WithMessage("Chat ID is required");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required");
    }
}

public class LeaveChatCommandHandler(IChatsRepository chatsRepository, IChatNotificationService notificationService) : IRequestHandler<LeaveChatCommand>
{
    public async Task<IOperationResult> HandleAsync(LeaveChatCommand request, CancellationToken cancellationToken = default)
    {
        var chat = await chatsRepository.GetByIdWithUsersAsync(request.ChatId, cancellationToken);
        if (chat == null)
        {
            return ResultsHelper.NotFound("Chat not found");
        }

        var userToRemove = chat.Users.FirstOrDefault(u => u.Id == request.UserId);
        if (userToRemove == null)
        {
            return ResultsHelper.BadRequest("User is not a member of this chat");
        }

        if (chat.AdminId == request.UserId && chat.Users.Count > 1)
        {
            return ResultsHelper.BadRequest("Admin cannot leave the chat. Please transfer admin rights first or remove all other users.");
        }

        chat.Users.Remove(userToRemove);

        if (chat.Users.Count == 0)
        {
            chatsRepository.Delete(chat);
        }
        else
        {
            chatsRepository.Update(chat);
        }

        await chatsRepository.SaveChangesAsync(cancellationToken);

        await notificationService.NotifyUserLeftChatAsync(chat.Id, userToRemove.Id, userToRemove.Username, chat.Name);

        return ResultsHelper.Ok(new { Message = "Successfully left the chat" });
    }
} 