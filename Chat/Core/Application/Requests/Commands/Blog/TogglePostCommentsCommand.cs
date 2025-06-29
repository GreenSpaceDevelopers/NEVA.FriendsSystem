using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using FluentValidation;

namespace Application.Requests.Commands.Blog;

public record TogglePostCommentsRequest(Guid PostId, Guid UserId) : IRequest;

public class TogglePostCommentsRequestHandler(IBlogRepository blogRepository) : IRequestHandler<TogglePostCommentsRequest>
{
    public async Task<IOperationResult> HandleAsync(TogglePostCommentsRequest request, CancellationToken cancellationToken = default)
    {
        var post = await blogRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null)
        {
            return ResultsHelper.NotFound("Post not found");
        }

        if (post.AuthorId != request.UserId)
        {
            return ResultsHelper.Forbidden("Only post author can toggle comments");
        }

        post.IsCommentsEnabled = !post.IsCommentsEnabled;
        await blogRepository.SaveChangesAsync(cancellationToken);

        return ResultsHelper.Ok(new { CommentsEnabled = post.IsCommentsEnabled });
    }
}

public class TogglePostCommentsRequestValidator : AbstractValidator<TogglePostCommentsRequest>
{
    public TogglePostCommentsRequestValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("PostId is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}