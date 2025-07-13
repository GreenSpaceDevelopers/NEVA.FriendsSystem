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

namespace Application.Requests.Commands.Posts;

public class AddPostRequest : IRequest
{
    public IFormFileCollection? Files { get; set; }
    public Guid? UserId { get; set; }
    public string? Content { get; set; }
    public string? Title { get; set; } = string.Empty;
}

public class AddPostRequestHandler (
    IBlogRepository blogRepository,
    IFilesStorage filesStorage,
    IFilesValidator filesValidator,
    IAttachmentsRepository attachments)
    : IRequestHandler<AddPostRequest>
{
    public async Task<IOperationResult> HandleAsync(AddPostRequest request, CancellationToken cancellationToken = default)
    {
        var user = await blogRepository.GetUserByIdWithPostsAsync(request.UserId ?? Guid.Empty, cancellationToken);
        if (user is null)
        {
            return ResultsHelper.NotFound("User not found");
        }

        var attachmentsList = new List<Attachment>();

        if (request.Files is not null && request.Files.Count > 0)
        {
            if (request.Files.Count > 10)
            {
                return ResultsHelper.BadRequest("Maximum 10 files allowed");
            }

            foreach (var file in request.Files)
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream, cancellationToken);

                if (filesValidator.ValidateFile(memoryStream, file.FileName) is not true)
                {
                    return ResultsHelper.BadRequest($"Invalid file: {file.FileName}");
                }

                var uploadResult = await filesStorage.UploadAsync(memoryStream, file.FileName, cancellationToken);

                if (!uploadResult.IsSuccess)
                {
                    return ResultsHelper.BadRequest($"File upload failed: {file.FileName}");
                }
                
                var fileResult = uploadResult.GetValue<FileUploadResult>();
                var type = await attachments.GetAttachmentTypeAsync(AttachmentTypes.Image, cancellationToken);

                var attachment = new Attachment
                {
                    Id = Guid.NewGuid(),
                    Url = fileResult.Url,
                    FileId = fileResult.FileId,
                    Type = type,
                    TypeId = type.Id,
                };
                
                await attachments.AddAsync(attachment, cancellationToken);
                attachmentsList.Add(attachment);
            }
        }

        var post = new Post
        {
            Id = Guid.NewGuid(),
            Author = user,
            AuthorId = user.Id,
            Content = request.Content ?? string.Empty,
            Title = request.Title,
            Attachments = attachmentsList,
            CreatedAt = DateTime.UtcNow,
            IsCommentsEnabled = true
        };

        await blogRepository.AddAsync(post, cancellationToken);
        await blogRepository.SaveChangesAsync(cancellationToken);

        return ResultsHelper.Created(post.Id);
    }
}

public class AddPostRequestValidator : AbstractValidator<AddPostRequest>
{
    public AddPostRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MinimumLength(10).WithMessage("Content must be at least 10 characters long.")
            .When(x => x.Content is not null);

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.")
            .Must(id => id != Guid.Empty).WithMessage("UserId must not be empty.");

        RuleFor(x => x.Files)
            .Must(files => files is not { Count: > 10 }).WithMessage("Maximum 10 files allowed.");
    }
}