using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using FluentValidation;

namespace Application.Requests.Commands.Posts;

public record TogglePinPostRequest(Guid PostId, Guid? UserId) : IRequest;

public class TogglePinPostRequestHandler(IBlogRepository blogRepository) : IRequestHandler<TogglePinPostRequest>
{
    public async Task<IOperationResult> HandleAsync(TogglePinPostRequest request, CancellationToken cancellationToken = default)
    {
        var user = await blogRepository.GetUserByIdWithPostsAsync(request.UserId ?? Guid.Empty, cancellationToken);
        if (user is null)
        {
            return ResultsHelper.NotFound("User not found");
        }

        var post = user.Posts.FirstOrDefault(post => post.Id == request.PostId);
        if (post is null)
        {
            return ResultsHelper.NotFound("Post not found");
        }

        post.IsPinned = !post.IsPinned;

        await blogRepository.SaveChangesAsync(cancellationToken);

        return ResultsHelper.Ok(new { PostId = post.Id, IsPinned = post.IsPinned });
    }
}

public class TogglePinPostRequestValidator : AbstractValidator<TogglePinPostRequest>
{
    public TogglePinPostRequestValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("Post Id is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}