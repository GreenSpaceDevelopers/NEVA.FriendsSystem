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
    public List<Guid> AttachmentsToDelete { get; set; } = new();
    public IFormFileCollection? NewFiles { get; set; }
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

        if (request.AttachmentsToDelete.Count > 0)
        {
            var attachmentsToDelete = post.Attachments
                .Where(att => request.AttachmentsToDelete.Contains(att.Id))
                .ToList();
            
            if (attachmentsToDelete.Count > 0)
            {
                var fileIdsToDelete = attachmentsToDelete
                    .Where(att => !string.IsNullOrEmpty(att.FileId))
                    .Select(att => att.FileId)
                    .ToList();
                
                if (fileIdsToDelete.Count > 0)
                {
                    await filesStorage.DeleteBatchByFileIdsAsync(fileIdsToDelete, cancellationToken);
                }
                
                foreach (var attachment in attachmentsToDelete)
                {
                    post.Attachments.Remove(attachment);
                    attachmentsRepository.Delete(attachment);
                }
            }
        }

        if (request.NewFiles is not null && request.NewFiles.Count > 0)
        {
            var totalAttachments = post.Attachments.Count + request.NewFiles.Count;
            if (totalAttachments > 10)
            {
                return ResultsHelper.BadRequest("Maximum 10 files allowed");
            }

            foreach (var file in request.NewFiles)
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream, cancellationToken);
                if (!filesValidator.ValidateFile(memoryStream, file.FileName))
                    return ResultsHelper.BadRequest($"Invalid file: {file.FileName}");

                var uploadResult = await filesStorage.UploadAsync(memoryStream, file.FileName, cancellationToken);
                if (!uploadResult.IsSuccess)
                    return ResultsHelper.BadRequest($"File upload failed: {file.FileName}");

                var fileResult = uploadResult.GetValue<FileUploadResult>();
                var type = await attachmentsRepository.GetAttachmentTypeAsync(AttachmentTypes.Image, cancellationToken);
                var attachment = new Attachment
                {
                    Id = Guid.NewGuid(),
                    Url = fileResult.Url,
                    FileId = fileResult.FileId,
                    Type = type,
                    TypeId = type.Id,
                };
                await attachmentsRepository.AddAsync(attachment, cancellationToken);
                post.Attachments.Add(attachment);
            }
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
        RuleFor(x => x.NewFiles)
            .Must(files => files is not { Count: > 10 }).WithMessage("Maximum 10 files allowed.");
    }
} 