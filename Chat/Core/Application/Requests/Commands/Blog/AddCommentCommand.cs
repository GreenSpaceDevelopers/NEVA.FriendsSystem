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
using Application.Abstractions.Services.Notifications;
using Application.Notifications;

namespace Application.Requests.Commands.Blog;

public record AddCommentRequest(Guid PostId, Guid UserId, string Content, IFormFileCollection? Attachments, Guid? ParentCommentId) : IRequest;

public class AddCommentRequestHandler(
    IBlogRepository blogRepository,
    IAttachmentsRepository attachmentsRepository,
    IFilesStorage filesStorage,
    IFilesValidator filesValidator,
    IChatUsersRepository chatUsersRepository,
    IBackendNotificationService notificationService) : IRequestHandler<AddCommentRequest>
{
    public async Task<IOperationResult> HandleAsync(AddCommentRequest request, CancellationToken cancellationToken = default)
    {
        var post = await blogRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null)
        {
            return ResultsHelper.NotFound("Post not found");
        }

        if (!post.IsCommentsEnabled)
        {
            return ResultsHelper.BadRequest("Comments are disabled for this post");
        }

        var isBlocked = await chatUsersRepository.IsUserBlockedByAsync(request.UserId, post.AuthorId, cancellationToken);
        if (isBlocked)
        {
            return ResultsHelper.Forbidden("You are blocked by the post author");
        }

        if (request.ParentCommentId.HasValue)
        {
            var parentComment = post.Comments.FirstOrDefault(c => c.Id == request.ParentCommentId.Value);
            if (parentComment is null)
            {
                return ResultsHelper.NotFound("Parent comment not found");
            }
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

                var fileResult = uploadResult.GetValue<FileUploadResult>();
                var attachmentType = await attachmentsRepository.GetAttachmentTypeAsync(AttachmentTypes.Image, cancellationToken);
                var attachment = new Attachment
                {
                    Id = Guid.NewGuid(),
                    Url = fileResult.Url,
                    FileId = fileResult.FileId,
                    Type = attachmentType,
                    TypeId = attachmentType.Id,
                };

                await attachmentsRepository.AddAsync(attachment, cancellationToken);
                attachmentsList.Add(attachment);
            }
        }

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            Content = request.Content,
            AuthorId = request.UserId,
            PostId = request.PostId,
            ParentCommentId = request.ParentCommentId,
            Attachments = attachmentsList,
            CreatedAt = DateTime.UtcNow
        };

        await blogRepository.AddCommentAsync(comment, cancellationToken);
        await blogRepository.SaveChangesAsync(cancellationToken);

        if (post.AuthorId == request.UserId)
        {
            return ResultsHelper.Created(comment.Id);
        }
        
        var commenter = await chatUsersRepository.GetByIdWithProfileDataAsync(request.UserId, cancellationToken);
        if (commenter is null)
        {
            return ResultsHelper.Created(comment.Id);
        }
        
        var receiverParams = new List<string> { "#", commenter.Username ?? commenter.AspNetUser.UserName };
        await notificationService.SendNotificationAsync(
            NotificationTemplateIds.PostComment,
            post.AuthorId,
            request.UserId,
            false,
            receiverParams,
            null);

        return ResultsHelper.Created(comment.Id);
    }
}

public class AddCommentRequestValidator : AbstractValidator<AddCommentRequest>
{
    public AddCommentRequestValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("PostId is required.");

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