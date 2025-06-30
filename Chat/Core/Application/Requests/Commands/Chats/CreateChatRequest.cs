using System.Collections.Concurrent;
using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Domain.Models.Messaging;
using Domain.Models.Users;
using FluentValidation;

namespace Application.Requests.Commands.Chats;

public record CreateChatRequest(Guid[] UserIds) : IRequest;

public class CreateChatRequestValidator : AbstractValidator<CreateChatRequest>
{
    public CreateChatRequestValidator()
    {
        RuleFor(x => x.UserIds).NotEmpty();
        RuleFor(x => x.UserIds.Length).Must(c => c < 100);
    }
}

public class CreateChatRequestHandler(IChatUsersRepository chatUsersRepository, IChatsRepository chatsRepository) : IRequestHandler<CreateChatRequest>
{
    public async Task<IOperationResult> HandleAsync(CreateChatRequest request, CancellationToken cancellationToken = default)
    {
        ConcurrentBag<ChatUser> chatUsers = [];

        await Parallel.ForEachAsync(request.UserIds, cancellationToken, async  (userId, ct) =>
        {
            var user = await chatUsersRepository.GetByIdAsync(userId, ct);
            
            if (user is null)
            {
                return;
            }

            chatUsers.Add(user);
        });
        
        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Users = chatUsers.ToList()
        };
        
        await chatsRepository.AddAsync(chat, cancellationToken);
        await chatsRepository.SaveChangesAsync(cancellationToken);
        
        return ResultsHelper.Created(chat.Id);
    }
}