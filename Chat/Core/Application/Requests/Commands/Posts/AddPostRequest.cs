using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Persistence.Repositories.Media;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Domain.Models.Blog;
using Domain.Models.Messaging;
using Microsoft.AspNetCore.Http;

namespace Application.Requests.Commands.Posts;

public record AddPostRequest(IFormFile File, Guid UserId, string Content, string Title) : IRequest;

public class AddPostRequestHandler
    (IBlogRepository blogRepository,
        IFilesStorage filesStorage,
        IFilesValidator filesValidator,
        IAttachmentsRepository attachments) : IRequestHandler<AddPostRequest>
{
    public async Task<IOperationResult> HandleAsync(AddPostRequest request, CancellationToken cancellationToken = default)
    {
        var user = await blogRepository.GetUserByIdWithPostsAsync(request.UserId, cancellationToken);
        
        if (user is null)
        {
            return ResultsHelper.NotFound("User not found");
        }
        
        using var memoryStream = new MemoryStream();
        await request.File.CopyToAsync(memoryStream, cancellationToken);

        if (filesValidator.ValidateFile(memoryStream, request.File.FileName) is not true)
        {
            return ResultsHelper.BadRequest("Invalid file");
        }

        var uploadResult = await filesStorage.UploadAsync(memoryStream, request.File.FileName, cancellationToken);

        if (!uploadResult.IsSuccess)
        {
            return ResultsHelper.BadRequest(uploadResult.GetValue<string>());
        }
        
        var type = await attachments.GetAttachmentTypeAsync(AttachmentTypes.Image, cancellationToken);
        
        var attachment = new Attachment
        {
            Id = Guid.NewGuid(),
            Url = uploadResult.GetValue<string>(),
            Type = type,
            TypeId = type.Id,
        };

        await attachments.AddAsync(attachment, cancellationToken);
        
        var post = new Post
        {
            Id = Guid.NewGuid(),
            Author = user,
            AuthorId = user.Id,
            Content = request.Content,
            Title = request.Title,
            AttachmentId = attachment.Id
        };
        
        await blogRepository.AddAsync(post, cancellationToken);
        await blogRepository.SaveChangesAsync(cancellationToken);
        
        return ResultsHelper.Created(post.Id);
    }
}
