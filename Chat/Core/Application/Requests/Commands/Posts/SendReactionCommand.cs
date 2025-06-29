using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Persistence.Repositories.Media;
using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Domain.Models.Messaging;

namespace Application.Requests.Commands.Posts;

public record SendReactionCommand(Guid PostId, Guid ReactionTypeId, Guid UserId) : IRequest;

public class SendReactionAsyncHandler(IBlogRepository blogRepository, IReactionsTypesRepository reactionsTypesRepository, IChatUsersRepository chatUsersRepository) : IRequestHandler<SendReactionCommand>
{
    public async Task<IOperationResult> HandleAsync(SendReactionCommand request, CancellationToken cancellationToken = default)
    {
        var reactionType = await reactionsTypesRepository.GetByIdAsync(request.ReactionTypeId, cancellationToken);

        if (reactionType is null || reactionType.Name != nameof(Reactions.PostReaction))
        {
            return ResultsHelper.BadRequest("Invalid reaction type");
        }

        var post = await blogRepository.GetByIdAsync(request.PostId, cancellationToken);

        if (post is null)
        {
            return ResultsHelper.NotFound("Post not found");
        }

        var user = await chatUsersRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return ResultsHelper.NotFound("User not found");
        }

        if (post.Reactions.Any(reaction => reaction.UserId == request.UserId))
        {
            return ResultsHelper.BadRequest("User has already reacted");
        }

        var reaction = new PostReaction
        {
            Id = Guid.NewGuid(),
            PostId = request.PostId,
            ReactionTypeId = request.ReactionTypeId,
            UserId = request.UserId,
            Reactor = user
        };

        post.Reactions.Add(reaction);
        await blogRepository.SaveChangesAsync(cancellationToken);

        return ResultsHelper.NoContent();
    }
}