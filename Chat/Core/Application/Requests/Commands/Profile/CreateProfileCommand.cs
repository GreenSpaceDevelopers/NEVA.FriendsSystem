using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Domain.Models.Users;

namespace Application.Requests.Commands.Profile;

public record CreateProfileCommand(AspNetUser AspNetUser) : IRequest;

public class CreateProfileCommandHandler(IChatUsersRepository chatUsersRepository, IPrivacyRepository privacyRepository) : IRequestHandler<CreateProfileCommand>
{
    public async Task<IOperationResult> HandleAsync(CreateProfileCommand request, CancellationToken cancellationToken = default)
    {
        var chatUser = new ChatUser(request.AspNetUser);

        var userPrivacySetting = new PrivacySetting
        {
            ChatUserId = chatUser.Id,
            SettingName = "Public"
        };

        chatUser.PrivacySetting = userPrivacySetting;

        await chatUsersRepository.AddAsync(chatUser, cancellationToken);
        await chatUsersRepository.SaveChangesAsync(cancellationToken);

        return ResultsHelper.NoContent();
    }
}