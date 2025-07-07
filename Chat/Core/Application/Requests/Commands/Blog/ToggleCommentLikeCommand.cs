using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Persistence.Repositories.Media;
using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using Domain.Models.Messaging;
using FluentValidation;
using Application.Abstractions.Services.Notifications;
using NEVA.BackendApi.Constants;

namespace Application.Requests.Commands.Blog;

public record ToggleCommentLikeRequest(Guid CommentId, Guid UserId, Guid ReactionTypeId) : IRequest;

public class ToggleCommentLikeRequestHandler(IBlogRepository blogRepository, IReactionsTypesRepository reactionsTypesRepository, IChatUsersRepository chatUsersRepository, IBackendNotificationService notificationService) : IRequestHandler<ToggleCommentLikeRequest>
{
    public async Task<IOperationResult> HandleAsync(ToggleCommentLikeRequest request, CancellationToken cancellationToken = default)
    {
        var comment = await blogRepository.GetCommentByIdAsync(request.CommentId, cancellationToken);
        
        if (comment is null)
        {
            return ResultsHelper.NotFound("Comment not found");
        }

        var isBlocked = await chatUsersRepository.IsUserBlockedByAsync(request.UserId, comment.AuthorId, cancellationToken);
        if (isBlocked)
        {
            return ResultsHelper.Forbidden("You are blocked by the comment author");
        }

        var existingLike = comment.CommentReactions?.FirstOrDefault(r => r.ReactorId == request.UserId && r.ReactionTypeId == request.ReactionTypeId);

        if (existingLike is not null)
        {
            await blogRepository.RemoveCommentReactionAsync(existingLike, cancellationToken);
            await blogRepository.SaveChangesAsync(cancellationToken);
            return ResultsHelper.Ok(new { Liked = false, ReactionTypeId = request.ReactionTypeId });
        }

        var newLike = new CommentReaction
        {
            Id = Guid.NewGuid(),
            CommentId = request.CommentId,
            ReactorId = request.UserId,
            ReactionTypeId = request.ReactionTypeId,
            CreatedAt = DateTime.UtcNow
        };
        
        var reactionType = await reactionsTypesRepository.GetByIdAsync(request.ReactionTypeId, cancellationToken);

        if (reactionType is null)
        {
            return ResultsHelper.BadRequest("Invalid reaction type");
        }

        newLike.ReactionType = reactionType;
        await blogRepository.AddCommentReactionAsync(newLike, cancellationToken);
        await blogRepository.SaveChangesAsync(cancellationToken);

        // Send notification to comment author if not self
        if (comment.AuthorId != request.UserId)
        {
            var reactor = await chatUsersRepository.GetByIdWithProfileDataAsync(request.UserId, cancellationToken);
            if (reactor is not null)
            {
                var receiverParams = new List<string> { "#", reactor.Username ?? reactor.AspNetUser.UserName, "комментарий" };
                await notificationService.SendNotificationAsync(
                    NotificationTemplatesConsts.PostReaction.Id,
                    comment.AuthorId,
                    request.UserId,
                    false,
                    receiverParams,
                    null);
            }
        }

        return ResultsHelper.Ok(new { Liked = true, ReactionTypeId = request.ReactionTypeId });
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