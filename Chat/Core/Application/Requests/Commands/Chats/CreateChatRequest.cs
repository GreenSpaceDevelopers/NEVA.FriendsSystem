using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using Domain.Models.Messaging;
using Domain.Models.Users;
using FluentValidation;

namespace Application.Requests.Commands.Chats;

public record CreateChatRequest(Guid CurrentUserId, Guid[] UserIds) : IRequest;

public class CreateChatRequestValidator : AbstractValidator<CreateChatRequest>
{
    public CreateChatRequestValidator()
    {
        RuleFor(x => x.CurrentUserId).NotEmpty();
        RuleFor(x => x.UserIds).NotNull();
        RuleFor(x => x.UserIds.Length).Must(c => c < 100);
    }
}

public class CreateChatRequestHandler(IChatUsersRepository chatUsersRepository, IChatsRepository chatsRepository) : IRequestHandler<CreateChatRequest>
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

        var adminId = request.CurrentUserId;

        var chatName = string.Join(", ", chatUsers.Select(u => u.Username));

        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Name = chatName,
            AdminId = adminId,
            Users = chatUsers.ToList()
        };
        
        await chatsRepository.AddAsync(chat, cancellationToken);
        await chatsRepository.SaveChangesAsync(cancellationToken);
        
        return ResultsHelper.Created(chat.Id);
    }
}