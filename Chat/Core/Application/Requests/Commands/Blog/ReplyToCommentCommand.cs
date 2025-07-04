using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Persistence.Repositories.Media;
using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using Domain.Models.Blog;
using Domain.Models.Messaging;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Application.Requests.Commands.Blog;

public record ReplyToCommentRequest(Guid CommentId, Guid UserId, string Content, IFormFile? Attachment) : IRequest;

public class ReplyToCommentRequestHandler(
    IBlogRepository blogRepository,
    IAttachmentsRepository attachmentsRepository,
    IFilesStorage filesStorage,
    IFilesValidator filesValidator,
    IChatUsersRepository chatUsersRepository) : IRequestHandler<ReplyToCommentRequest>
{
    public async Task<IOperationResult> HandleAsync(ReplyToCommentRequest request, CancellationToken cancellationToken = default)
    {
        var parentComment = await blogRepository.GetCommentByIdAsync(request.CommentId, cancellationToken);
        if (parentComment is null)
        {
            return ResultsHelper.NotFound("Comment not found");
        }

        var post = await blogRepository.GetByIdAsync(parentComment.PostId, cancellationToken);
        if (post is null)
        {
            return ResultsHelper.NotFound("Post not found");
        }

        if (!post.IsCommentsEnabled)
        {
            return ResultsHelper.BadRequest("Comments are disabled for this post");
        }

        var isBlockedByPostAuthor = await chatUsersRepository.IsUserBlockedByAsync(request.UserId, post.AuthorId, cancellationToken);
        if (isBlockedByPostAuthor)
        {
            return ResultsHelper.Forbidden("You are blocked by the post author");
        }

        var isBlockedByCommentAuthor = await chatUsersRepository.IsUserBlockedByAsync(request.UserId, parentComment.AuthorId, cancellationToken);
        if (isBlockedByCommentAuthor)
        {
            return ResultsHelper.Forbidden("You are blocked by the comment author");
        }

        Attachment? attachment = null;
        if (request.Attachment is not null)
        {
            using var stream = new MemoryStream();
            await request.Attachment.CopyToAsync(stream, cancellationToken);

            if (filesValidator.ValidateFile(stream, request.Attachment.FileName) is not true)
            {
                return ResultsHelper.BadRequest("Invalid attachment file");
            }

            var uploadResult = await filesStorage.UploadAsync(stream, request.Attachment.FileName, cancellationToken);
            if (!uploadResult.IsSuccess)
            {
                return ResultsHelper.BadRequest(uploadResult.GetValue<string>());
            }

            var attachmentType = await attachmentsRepository.GetAttachmentTypeAsync(AttachmentTypes.Image, cancellationToken);
            attachment = new Attachment
            {
                Id = Guid.NewGuid(),
                Url = uploadResult.GetValue<string>(),
                Type = attachmentType,
                TypeId = attachmentType.Id,
            };

            await attachmentsRepository.AddAsync(attachment, cancellationToken);
        }

        var reply = new Comment
        {
            Id = Guid.NewGuid(),
            Content = request.Content,
            AuthorId = request.UserId,
            PostId = post.Id,
            ParentCommentId = request.CommentId,
            Attachment = attachment,
            CreatedAt = DateTime.UtcNow
        };

        await blogRepository.AddCommentAsync(reply, cancellationToken);
        await blogRepository.SaveChangesAsync(cancellationToken);

        return ResultsHelper.Created(reply.Id);
    }
}

public class ReplyToCommentRequestValidator : AbstractValidator<ReplyToCommentRequest>
{
    public ReplyToCommentRequestValidator()
    {
        RuleFor(x => x.CommentId)
            .NotEmpty().WithMessage("CommentId is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MinimumLength(1).WithMessage("Content must not be empty.")
            .MaximumLength(1000).WithMessage("Content must not exceed 1000 characters.");

        RuleFor(x => x.Attachment)
            .Must(file => file == null || file.Length > 0).WithMessage("Attachment file must not be empty.");
    }
}