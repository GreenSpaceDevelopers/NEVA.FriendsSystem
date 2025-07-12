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

public record ReplyToCommentRequest(Guid CommentId, Guid UserId, string Content, IFormFileCollection? Attachments) : IRequest;

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

        var attachmentsList = new List<Attachment>();

        if (request.Attachments is not null && request.Attachments.Count > 0)
        {
            if (request.Attachments.Count > 10)
            {
                return ResultsHelper.BadRequest("Maximum 10 files allowed");
            }

            foreach (var file in request.Attachments)
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream, cancellationToken);

                if (filesValidator.ValidateFile(stream, file.FileName) is not true)
                {
                    return ResultsHelper.BadRequest($"Invalid attachment file: {file.FileName}");
                }

                var uploadResult = await filesStorage.UploadAsync(stream, file.FileName, cancellationToken);
                if (!uploadResult.IsSuccess)
                {
                    return ResultsHelper.BadRequest($"File upload failed: {file.FileName}");
                }

                var attachmentType = await attachmentsRepository.GetAttachmentTypeAsync(AttachmentTypes.Image, cancellationToken);
                var attachment = new Attachment
                {
                    Id = Guid.NewGuid(),
                    Url = uploadResult.GetValue<string>(),
                    Type = attachmentType,
                    TypeId = attachmentType.Id,
                };

                await attachmentsRepository.AddAsync(attachment, cancellationToken);
                attachmentsList.Add(attachment);
            }
        }

        var reply = new Comment
        {
            Id = Guid.NewGuid(),
            Content = request.Content,
            AuthorId = request.UserId,
            PostId = post.Id,
            ParentCommentId = request.CommentId,
            Attachments = attachmentsList,
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

        RuleFor(x => x.Attachments)
            .Must(files => files is not { Count: > 10 }).WithMessage("Maximum 10 files allowed.");
    }
}