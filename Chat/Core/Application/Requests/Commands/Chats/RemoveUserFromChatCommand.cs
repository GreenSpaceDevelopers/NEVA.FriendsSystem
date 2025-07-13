using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using FluentValidation;

namespace Application.Requests.Commands.Chats;

public record RemoveUserFromChatCommand(Guid ChatId, Guid AdminId, Guid UserIdToRemove) : IRequest;

public class RemoveUserFromChatCommandValidator : AbstractValidator<RemoveUserFromChatCommand>
{
    public RemoveUserFromChatCommandValidator()
    {
        RuleFor(x => x.ChatId).NotEmpty().WithMessage("Chat ID is required");
        RuleFor(x => x.AdminId).NotEmpty().WithMessage("Admin ID is required");
        RuleFor(x => x.UserIdToRemove).NotEmpty().WithMessage("User ID to remove is required");
        RuleFor(x => x.AdminId).NotEqual(x => x.UserIdToRemove).WithMessage("Admin cannot remove themselves");
    }
}

public class RemoveUserFromChatCommandHandler(IChatsRepository chatsRepository) : IRequestHandler<RemoveUserFromChatCommand>
{
    public async Task<IOperationResult> HandleAsync(RemoveUserFromChatCommand request, CancellationToken cancellationToken = default)
    {
        var chat = await chatsRepository.GetByIdWithUsersAsync(request.ChatId, cancellationToken);
        if (chat == null)
        {
            return ResultsHelper.NotFound("Chat not found");
        }

        if (chat.AdminId != request.AdminId)
        {
            return ResultsHelper.Forbidden("Only chat admin can remove users");
        }

        if (chat.Users.All(u => u.Id != request.AdminId))
        {
            return ResultsHelper.Forbidden("Admin is not a member of this chat");
        }

        var userToRemove = chat.Users.FirstOrDefault(u => u.Id == request.UserIdToRemove);
        if (userToRemove == null)
        {
            return ResultsHelper.BadRequest("User to remove is not a member of this chat");
        }

        chat.Users.Remove(userToRemove);

        chatsRepository.Update(chat);
        await chatsRepository.SaveChangesAsync(cancellationToken);

        return ResultsHelper.Ok(new { Message = "User successfully removed from chat" });
    }
} 