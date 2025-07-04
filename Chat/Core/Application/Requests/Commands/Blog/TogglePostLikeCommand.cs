using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Persistence.Repositories.Media;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using Domain.Models.Messaging;
using FluentValidation;

namespace Application.Requests.Commands.Blog;

public record TogglePostLikeRequest(Guid PostId, Guid UserId, Guid ReactionTypeId) : IRequest;

public class TogglePostLikeRequestHandler(IBlogRepository blogRepository, IReactionsTypesRepository reactionsTypesRepository) : IRequestHandler<TogglePostLikeRequest>
{
    public async Task<IOperationResult> HandleAsync(TogglePostLikeRequest request, CancellationToken cancellationToken = default)
    {
        var post = await blogRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null)
        {
            return ResultsHelper.NotFound("Post not found");
        }

        var existingLike = post.Reactions?.FirstOrDefault(r => r.UserId == request.UserId);

        if (existingLike is not null)
        {
            await blogRepository.RemovePostReactionAsync(existingLike, cancellationToken);
            await blogRepository.SaveChangesAsync(cancellationToken);
            return ResultsHelper.Ok(new { Liked = false });
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
            UserId = request.UserId,
            CreatedAt = DateTime.UtcNow,
            ReactionType = reactionType
        };

        await blogRepository.AddPostReactionAsync(newLike, cancellationToken);
        await blogRepository.SaveChangesAsync(cancellationToken);
        return ResultsHelper.Ok(new { Liked = true });
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