using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using Microsoft.Extensions.Logging;

namespace Application.Requests.Commands.Privacy;

public record CreatePrivacySettingsForAllUsersCommand : IRequest;

public class CreatePrivacySettingsForAllUsersCommandHandler(
    IChatUsersRepository chatUsersRepository,
    IPrivacyRepository privacyRepository,
    ILogger<CreatePrivacySettingsForAllUsersCommandHandler> logger) : IRequestHandler<CreatePrivacySettingsForAllUsersCommand>
{
    public async Task<IOperationResult> HandleAsync(CreatePrivacySettingsForAllUsersCommand request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting creation of privacy settings for all users");

        var allUsers = await chatUsersRepository.GetAllUsersAsync(cancellationToken);
        logger.LogInformation("Found {UserCount} users", allUsers.Count);

        var createdCount = 0;
        var skippedCount = 0;

        foreach (var user in allUsers)
        {
            try
            {
                var existingSettings = await privacyRepository.GetUserPrivacySettingsAsync(user.Id, cancellationToken);
                
                if (existingSettings != null)
                {
                    skippedCount++;
                    continue;
                }

                await privacyRepository.CreateDefaultPrivacySettingsAsync(user.Id, cancellationToken);
                createdCount++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create privacy settings for user {UserId}", user.Id);
            }
        }

        await privacyRepository.SaveChangesAsync(cancellationToken);

        var result = new
        {
            TotalUsers = allUsers.Count,
            CreatedSettings = createdCount,
            SkippedUsers = skippedCount,
            Message = $"Successfully created {createdCount} users. {skippedCount} users had settings."
        };

        return ResultsHelper.Ok(result);
    }
} 