using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Dtos.Requests.Messaging;
using Application.Dtos.Responses.Messaging;
using Application.Services.ApplicationInfrastructure.Results;
using FluentValidation;

namespace Application.Requests.Commands.Messaging;

public record UpdateUserChatSettingsCommand(Guid UserId, Guid ChatId, UpdateUserChatSettingsRequest Request) : IRequest;

public class UpdateUserChatSettingsCommandHandler(
    IUserChatSettingsRepository userChatSettingsRepository,
    IChatsRepository chatsRepository,
    IChatUsersRepository chatUsersRepository) : IRequestHandler<UpdateUserChatSettingsCommand>
{
    public async Task<IOperationResult> HandleAsync(UpdateUserChatSettingsCommand command, CancellationToken cancellationToken = default)
    {
        var request = command.Request;
        var userId = command.UserId;
        var chatId = command.ChatId;
        
        var chat = await chatsRepository.GetByIdWithUsersAsync(chatId, cancellationToken);
        if (chat == null)
        {
            return ResultsHelper.NotFound("Чат не найден");
        }

        if (chat.Users.All(u => u.Id != userId))
        {
            return ResultsHelper.Forbidden("Вы не являетесь участником данного чата");
        }

        var settings = await userChatSettingsRepository.GetByUserAndChatOrCreateAsync(
            userId, 
            chatId, 
            cancellationToken);

        if (request.IsMuted.HasValue)
            settings.IsMuted = request.IsMuted.Value;
            
        if (request.IsDisabled.HasValue)
            settings.IsDisabled = request.IsDisabled.Value;

        if (request.DisabledUserIds != null)
        {
            var disabledUsers = new List<Domain.Models.Users.ChatUser>();
            foreach (var disabledUserId in request.DisabledUserIds)
            {
                var user = await chatUsersRepository.GetByIdAsync(disabledUserId, cancellationToken);
                if (user != null && chat.Users.Any(u => u.Id == disabledUserId))
                {
                    disabledUsers.Add(user);
                }
            }
            settings.DisabledUsers = disabledUsers;
        }

        userChatSettingsRepository.UpdateAsync(settings, cancellationToken);
        await userChatSettingsRepository.SaveChangesAsync(cancellationToken);

        var response = new UserChatSettingsDto(
            settings.Id,
            settings.UserId,
            settings.ChatId,
            settings.IsMuted,
            settings.IsDisabled,
            settings.LastReadMessageId,
            settings.DisabledUsers?.Select(u => u.Id).ToList() ?? new List<Guid>()
        );

        return ResultsHelper.Ok(response);
    }
}

public class UpdateUserChatSettingsCommandValidator : AbstractValidator<UpdateUserChatSettingsCommand>
{
    public UpdateUserChatSettingsCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("ID пользователя обязателен");
            
        RuleFor(x => x.ChatId)
            .NotEmpty()
            .WithMessage("ID чата обязателен");
    }
} 