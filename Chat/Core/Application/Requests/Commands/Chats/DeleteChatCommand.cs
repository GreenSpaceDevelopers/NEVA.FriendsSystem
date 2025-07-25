using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using FluentValidation;

namespace Application.Requests.Commands.Chats;

public record DeleteChatCommand(Guid ChatId, Guid RequestingUserId) : IRequest;

public class DeleteChatCommandValidator : AbstractValidator<DeleteChatCommand>
{
    public DeleteChatCommandValidator()
    {
        RuleFor(x => x.ChatId).NotEmpty().WithMessage("Chat ID is required");
        RuleFor(x => x.RequestingUserId).NotEmpty().WithMessage("User ID is required");
    }
}

public class DeleteChatCommandHandler(IChatsRepository chatsRepository)
    : IRequestHandler<DeleteChatCommand>
{
    public async Task<IOperationResult> HandleAsync(DeleteChatCommand request, CancellationToken cancellationToken = default)
    {
        var chat = await chatsRepository.GetByIdWithUsersAsync(request.ChatId, cancellationToken);
        if (chat == null)
        {
            return ResultsHelper.NotFound("Chat not found");
        }

        if (chat.IsGroup)
        {
            if (chat.AdminId != request.RequestingUserId)
            {
                return ResultsHelper.Forbidden("Only chat admin can delete the chat");
            }
        }

        chatsRepository.Delete(chat);
        await chatsRepository.SaveChangesAsync(cancellationToken);

        return ResultsHelper.Ok(new { Message = "Chat successfully deleted" });
    }
} 