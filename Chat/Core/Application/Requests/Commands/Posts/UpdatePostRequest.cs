using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Persistence.Repositories.Media;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using Domain.Models.Messaging;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Application.Requests.Commands.Posts;

public class UpdatePostRequest : IRequest
{
    public Guid PostId { get; set; }
    public Guid? UserId { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public IFormFile? File { get; set; }
}

public class UpdatePostRequestHandler(
    IBlogRepository blogRepository,
    IFilesStorage filesStorage,
    IFilesValidator filesValidator,
    IAttachmentsRepository attachmentsRepository)
    : IRequestHandler<UpdatePostRequest>
{
    public async Task<IOperationResult> HandleAsync(UpdatePostRequest request, CancellationToken cancellationToken = default)
    {
        var post = await blogRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null)
            return ResultsHelper.NotFound("Post not found");

        if (post.AuthorId != request.UserId)
            return ResultsHelper.Forbidden("Only the author can update the post");

        if (request.Title != null)
            post.Title = request.Title;
        if (request.Content != null)
            post.Content = request.Content;

        if (request.File is not null)
        {
            using var memoryStream = new MemoryStream();
            await request.File.CopyToAsync(memoryStream, cancellationToken);
            if (!filesValidator.ValidateFile(memoryStream, request.File.FileName))
                return ResultsHelper.BadRequest("Invalid file");

            var uploadResult = await filesStorage.UploadAsync(memoryStream, request.File.FileName, cancellationToken);
            if (!uploadResult.IsSuccess)
                return ResultsHelper.BadRequest("File upload failed");

            var type = await attachmentsRepository.GetAttachmentTypeAsync(AttachmentTypes.Image, cancellationToken);
            var attachment = new Attachment
            {
                Id = Guid.NewGuid(),
                Url = uploadResult.GetValue<string>(),
                Type = type,
                BucketName = "chat-files",
                TypeId = type.Id,
            };
            await attachmentsRepository.AddAsync(attachment, cancellationToken);
            post.AttachmentId = attachment.Id;
            post.Attachment = attachment;
        }

        await blogRepository.SaveChangesAsync(cancellationToken);
        return ResultsHelper.NoContent();
    }
}

public class UpdatePostRequestValidator : AbstractValidator<UpdatePostRequest>
{
    public UpdatePostRequestValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("PostId is required.");
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
        RuleFor(x => x.Content)
            .MinimumLength(10).WithMessage("Content must be at least 10 characters long.")
            .When(x => x.Content != null);
    }
} 