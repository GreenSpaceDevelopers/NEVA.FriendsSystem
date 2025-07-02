using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Persistence.Repositories.Media;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using Domain.Models.Blog;
using Domain.Models.Messaging;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Application.Requests.Commands.Blog;

public record AddCommentRequest(Guid PostId, Guid UserId, string Content, IFormFile? Attachment, Guid? ParentCommentId) : IRequest;

public class AddCommentRequestHandler(
    IBlogRepository blogRepository,
    IAttachmentsRepository attachmentsRepository,
    IFilesStorage filesStorage,
    IFilesValidator filesValidator) : IRequestHandler<AddCommentRequest>
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

        if (request.ParentCommentId.HasValue)
        {
            var parentComment = post.Comments.FirstOrDefault(c => c.Id == request.ParentCommentId.Value);
            if (parentComment is null)
            {
                return ResultsHelper.NotFound("Parent comment not found");
            }
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

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            Text = request.Content,
            Content = request.Content,
            AuthorId = request.UserId,
            PostId = request.PostId,
            ParentId = request.ParentCommentId ?? Guid.Empty,
            ParentCommentId = request.ParentCommentId,
            Attachment = attachment,
            CreatedAt = DateTime.UtcNow
        };

        post.Comments.Add(comment);
        await blogRepository.SaveChangesAsync(cancellationToken);

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

        RuleFor(x => x.Attachment)
            .Must(file => file == null || file.Length > 0).WithMessage("Attachment file must not be empty.");
    }
}