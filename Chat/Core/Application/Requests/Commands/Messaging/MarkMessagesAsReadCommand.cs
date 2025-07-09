using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using FluentValidation;

namespace Application.Requests.Commands.Messaging;

public record MarkMessagesAsReadCommand(Guid UserId, Guid ChatId, Guid? LastReadMessageId = null) : IRequest;

public class MarkMessagesAsReadCommandHandler(
    IChatsRepository chatsRepository,
    IUserChatSettingsRepository userChatSettingsRepository) : IRequestHandler<MarkMessagesAsReadCommand>
{
    public async Task<IOperationResult> HandleAsync(MarkMessagesAsReadCommand request, CancellationToken cancellationToken = default)
    {
        var chat = await chatsRepository.GetByIdWithUsersAsync(request.ChatId, cancellationToken);
        if (chat == null)
        {
            return ResultsHelper.NotFound("Чат не найден");
        }

        if (chat.Users.All(u => u.Id != request.UserId))
        {
            return ResultsHelper.Forbidden("Вы не являетесь участником данного чата");
        }

        var lastReadMessageId = request.LastReadMessageId;
        if (lastReadMessageId == null)
        {
            var lastMessage = await chatsRepository.GetLastMessageInChatAsync(request.ChatId, cancellationToken);
            lastReadMessageId = lastMessage?.Id;
        }

        var userChatSettings = await userChatSettingsRepository.GetByUserAndChatOrCreateAsync(
            request.UserId, 
            request.ChatId, 
            cancellationToken);

        userChatSettings.LastReadMessageId = lastReadMessageId;

        userChatSettingsRepository.UpdateAsync(userChatSettings, cancellationToken);
        await userChatSettingsRepository.SaveChangesAsync(cancellationToken);

        return ResultsHelper.NoContent();
    }
}

public class MarkMessagesAsReadCommandValidator : AbstractValidator<MarkMessagesAsReadCommand>
{
    public MarkMessagesAsReadCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("ID пользователя обязателен");

        RuleFor(x => x.ChatId)
            .NotEmpty()
            .WithMessage("ID чата обязателен");
    }
} 