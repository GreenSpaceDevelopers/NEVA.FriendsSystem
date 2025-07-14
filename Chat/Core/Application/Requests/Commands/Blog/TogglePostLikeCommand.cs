using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Persistence.Repositories.Media;
using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using Domain.Models.Messaging;
using FluentValidation;
using Application.Abstractions.Services.External;
using Application.Notifications;

namespace Application.Requests.Commands.Blog;

public record TogglePostLikeRequest(Guid PostId, Guid UserId, Guid ReactionTypeId) : IRequest;

public class TogglePostLikeRequestHandler(IBlogRepository blogRepository, IReactionsTypesRepository reactionsTypesRepository, IChatUsersRepository chatUsersRepository, INevaBackendService nevaBackendService) : IRequestHandler<TogglePostLikeRequest>
{
    public async Task<IOperationResult> HandleAsync(TogglePostLikeRequest request,
        CancellationToken cancellationToken = default)
    {
        var post = await blogRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null)
        {
            return ResultsHelper.NotFound("Post not found");
        }

        var isBlocked =
            await chatUsersRepository.IsUserBlockedByAsync(request.UserId, post.AuthorId, cancellationToken);
        if (isBlocked)
        {
            return ResultsHelper.Forbidden("You are blocked by the post author");
        }

        var existingLike = post.Reactions?.FirstOrDefault(r =>
            r.ReactorId == request.UserId && r.ReactionTypeId == request.ReactionTypeId);

        if (existingLike is not null)
        {
            await blogRepository.RemovePostReactionAsync(existingLike, cancellationToken);
            await blogRepository.SaveChangesAsync(cancellationToken);
            return ResultsHelper.Ok(new { Liked = false, ReactionTypeId = request.ReactionTypeId });
        }

        var reactionType = await reactionsTypesRepository.GetByIdAsync(request.ReactionTypeId, cancellationToken);

        if (reactionType is null)
        {
            return ResultsHelper.BadRequest("Invalid reaction type");
        }

        var newLike = new PostReaction
        {
            Id = Guid.NewGuid(),
            PostId = request.PostId,
            ReactorId = request.UserId,
            ReactionTypeId = request.ReactionTypeId,
            CreatedAt = DateTime.UtcNow,
            ReactionType = reactionType
        };

        await blogRepository.AddPostReactionAsync(newLike, cancellationToken);
        await blogRepository.SaveChangesAsync(cancellationToken);

        if (post.AuthorId == request.UserId)
        {
            return ResultsHelper.Ok(new { Liked = true, ReactionTypeId = request.ReactionTypeId });
        }

        var reactor = await chatUsersRepository.GetByIdWithProfileDataAsync(request.UserId, cancellationToken);
        if (reactor is null)
        {
            return ResultsHelper.Ok(new { Liked = true, ReactionTypeId = request.ReactionTypeId });
        }
        
        var receiverParams = new List<string> { "#", reactor.Username ?? reactor.AspNetUser.UserName, "пост" };
        await nevaBackendService.SendNotificationAsync(
            NotificationTemplateIds.PostReaction,
            post.AuthorId,
            request.UserId,
            false,
            receiverParams,
            null,
            cancellationToken);

        return ResultsHelper.Ok(new { Liked = true, ReactionTypeId = request.ReactionTypeId });
    }
}

public class TogglePostLikeRequestValidator : AbstractValidator<TogglePostLikeRequest>
{
    public TogglePostLikeRequestValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("PostId is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}