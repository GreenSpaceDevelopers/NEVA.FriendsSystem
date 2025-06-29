using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Domain.Models.Messaging;
using FluentValidation;

namespace Application.Requests.Commands.Blog;

public record TogglePostLikeRequest(Guid PostId, Guid UserId) : IRequest;

public class TogglePostLikeRequestHandler(IBlogRepository blogRepository) : IRequestHandler<TogglePostLikeRequest>
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
            post.Reactions!.Remove(existingLike);
            await blogRepository.SaveChangesAsync(cancellationToken);
            return ResultsHelper.Ok(new { Liked = false });
        }
        else
        {
            var newLike = new PostReaction
            {
                Id = Guid.NewGuid(),
                PostId = request.PostId,
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow
            };

            post.Reactions ??= new List<PostReaction>();
            post.Reactions.Add(newLike);
            await blogRepository.SaveChangesAsync(cancellationToken);
            return ResultsHelper.Ok(new { Liked = true });
        }
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