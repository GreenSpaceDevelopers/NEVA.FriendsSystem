using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Domain.Models.Blog;
using Domain.Models.Messaging;
using FluentValidation;

namespace Application.Requests.Commands.Blog;

public record ToggleCommentLikeRequest(Guid CommentId, Guid UserId) : IRequest;

public class ToggleCommentLikeRequestHandler(IBlogRepository blogRepository) : IRequestHandler<ToggleCommentLikeRequest>
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
            comment.CommentReactions.Remove(existingLike);
            await blogRepository.SaveChangesAsync(cancellationToken);
            return ResultsHelper.Ok(new { Liked = false });
        }
        else
        {
            var newLike = new CommentReaction
            {
                Id = Guid.NewGuid(),
                CommentId = request.CommentId,
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow
            };

            comment.CommentReactions ??= new List<CommentReaction>();
            comment.CommentReactions.Add(newLike);
            await blogRepository.SaveChangesAsync(cancellationToken);
            return ResultsHelper.Ok(new { Liked = true });
        }
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