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
    public IFormFile? File { get; set; }
    public Guid? UserId { get; set; }
    public string? Content { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCommentsEnabled { get; set; }
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

        var uploadResult = ResultsHelper.Ok("no file");

        Attachment? attachment = null;
        if (request.File is not null)
        {
            using var memoryStream = new MemoryStream();
            await request.File.CopyToAsync(memoryStream, cancellationToken);

            if (filesValidator.ValidateFile(memoryStream, request.File.FileName) is not true)
            {
                return ResultsHelper.BadRequest("Invalid file");
            }

            uploadResult = await filesStorage.UploadAsync(memoryStream, request.File.FileName, cancellationToken);

            if (!uploadResult.IsSuccess)
            {
                return ResultsHelper.BadRequest("file upload failed");
            }
            
            var type = await attachments.GetAttachmentTypeAsync(AttachmentTypes.Image, cancellationToken);

            attachment = new Attachment
            {
                Id = Guid.NewGuid(),
                Url = uploadResult.GetValue<string>(),
                Type = type,
                TypeId = type.Id,
            };
            
            await attachments.AddAsync(attachment, cancellationToken);
        }

        var post = new Post
        {
            Id = Guid.NewGuid(),
            Author = user,
            AuthorId = user.Id,
            Content = request.Content ?? string.Empty,
            Title = request.Title,
            AttachmentId = attachment?.Id ?? null,
            CreatedAt = DateTime.UtcNow,
            IsCommentsEnabled = request.IsCommentsEnabled
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
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title must not exceed 100 characters.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MinimumLength(10).WithMessage("Content must be at least 10 characters long.")
            .When(x => x.Content is not null);

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.")
            .Must(id => id != Guid.Empty).WithMessage("UserId must not be empty.");
    }
}