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

        var privacySetting = await privacyRepository.GetPrivacySettingsAsync(cancellationToken);
        
        chatUser.PrivacySetting = privacySetting.First(p => p.Id == (int)PrivacySettingsEnums.Public);
        
        await chatUsersRepository.AddAsync(chatUser, cancellationToken);
        await chatUsersRepository.SaveChangesAsync(cancellationToken);
        
        return ResultsHelper.NoContent();
    }
}