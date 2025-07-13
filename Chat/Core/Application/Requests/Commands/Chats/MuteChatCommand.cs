using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using FluentValidation;

namespace Application.Requests.Commands.Chats;

public record MuteChatCommand(Guid ChatId, Guid UserId, bool IsMuted) : IRequest;

public class MuteChatCommandValidator : AbstractValidator<MuteChatCommand>
{
    public MuteChatCommandValidator()
    {
        RuleFor(x => x.ChatId).NotEmpty().WithMessage("Chat ID is required");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required");
    }
}

public class MuteChatCommandHandler(
    IChatsRepository chatsRepository,
    IUserChatSettingsRepository userChatSettingsRepository) : IRequestHandler<MuteChatCommand>
{
    public async Task<IOperationResult> HandleAsync(MuteChatCommand request, CancellationToken cancellationToken = default)
    {
        var chat = await chatsRepository.GetByIdWithUsersAsync(request.ChatId, cancellationToken);
        if (chat == null)
        {
            return ResultsHelper.NotFound("Chat not found");
        }

        if (chat.Users.All(u => u.Id != request.UserId))
        {
            return ResultsHelper.Forbidden("You are not a member of this chat");
        }

        var userChatSettings = await userChatSettingsRepository.GetByUserAndChatOrCreateAsync(
            request.UserId, 
            request.ChatId, 
            cancellationToken);

        userChatSettings.IsMuted = request.IsMuted;

        userChatSettingsRepository.UpdateAsync(userChatSettings, cancellationToken);
        await userChatSettingsRepository.SaveChangesAsync(cancellationToken);

        var action = request.IsMuted ? "muted" : "unmuted";
        return ResultsHelper.Ok(new { Message = $"Chat successfully {action}" });
    }
} 