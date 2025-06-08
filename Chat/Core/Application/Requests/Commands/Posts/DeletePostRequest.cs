using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using FluentValidation;

namespace Application.Requests.Commands.Posts;

public record DeletePostRequest(Guid Id, Guid UserId) : IRequest;

public class DeletePostRequestHandler(IBlogRepository blogRepository) : IRequestHandler<DeletePostRequest>
{
    public async Task<IOperationResult> HandleAsync(DeletePostRequest request, CancellationToken cancellationToken = default)
    {
        var user = await blogRepository.GetUserByIdWithPostsAsync(request.UserId, cancellationToken);
        
        if (user is null)
        {
            return ResultsHelper.NotFound("User not found");
        }
        
        var post = user.Posts.FirstOrDefault(post => post.Id == request.Id);
        
        if (post is null)
        {
            return ResultsHelper.NotFound("Post not found");
        }
        
        user.Posts.Remove(post);
        blogRepository.Delete(post);
        await blogRepository.SaveChangesAsync(cancellationToken);
        
        return ResultsHelper.NoContent();
    }
}

public class DeletePostRequestValidator : AbstractValidator<DeletePostRequest>
{
    public DeletePostRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Post Id is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}