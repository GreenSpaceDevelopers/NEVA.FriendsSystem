using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Persistence.Repositories.Media;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using Domain.Models.Messaging;
using FluentValidation;

namespace Application.Requests.Commands.Blog;

public record ToggleCommentLikeRequest(Guid CommentId, Guid UserId, Guid ReactionTypeId) : IRequest;

public class ToggleCommentLikeRequestHandler(IBlogRepository blogRepository, IReactionsTypesRepository reactionsTypesRepository) : IRequestHandler<ToggleCommentLikeRequest>
{
    public async Task<IOperationResult> HandleAsync(ToggleCommentLikeRequest request, CancellationToken cancellationToken = default)
    {
        var comment = await blogRepository.GetCommentByIdAsync(request.CommentId, cancellationToken);
        
        if (comment is null)
        {
            return ResultsHelper.NotFound("Comment not found");
        }

        var existingLike = comment.CommentReactions?.FirstOrDefault(r => r.UserId == request.UserId);

        if (existingLike is not null)
        {
            await blogRepository.RemoveCommentReactionAsync(existingLike, cancellationToken);
            await blogRepository.SaveChangesAsync(cancellationToken);
            return ResultsHelper.Ok(new { Liked = false });
        }

        var newLike = new CommentReaction
        {
            Id = Guid.NewGuid(),
            CommentId = request.CommentId,
            UserId = request.UserId,
            CreatedAt = DateTime.UtcNow
        };
        
        var reactionType = await reactionsTypesRepository.GetByIdAsync(request.ReactionTypeId, cancellationToken);

        if (reactionType is null)
        {
            return ResultsHelper.BadRequest("Invalid reaction type");
        }

        await blogRepository.AddCommentReactionAsync(newLike, cancellationToken);
        await blogRepository.SaveChangesAsync(cancellationToken);
        return ResultsHelper.Ok(new { Liked = true });
    }
}

public class ToggleCommentLikeRequestValidator : AbstractValidator<ToggleCommentLikeRequest>
{
    public ToggleCommentLikeRequestValidator()
    {
        RuleFor(x => x.CommentId)
            .NotEmpty().WithMessage("CommentId is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}