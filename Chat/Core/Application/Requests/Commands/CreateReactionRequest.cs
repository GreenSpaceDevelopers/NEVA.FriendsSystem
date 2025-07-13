using Application.Abstractions.Persistence.Repositories.Media;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using Domain.Models.Messaging;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Attachment = Domain.Models.Messaging.Attachment;

namespace Application.Requests.Commands;

public record CreateReactionRequest(IFormFile ReactionImage, string Name, string Description = "") : IRequest;

public class CreateReactionRequestHandler(
    IFilesStorage filesStorage,
    IFilesValidator filesValidator,
    IAttachmentsRepository attachments,
    IReactionsTypesRepository reactions) : IRequestHandler<CreateReactionRequest>
{
    public async Task<IOperationResult> HandleAsync(CreateReactionRequest request, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await request.ReactionImage.CopyToAsync(memoryStream, cancellationToken);

        if (filesValidator.ValidateFile(memoryStream, request.ReactionImage.FileName) is not true)
        {
            return ResultsHelper.BadRequest("Invalid file");
        }

        var uploadResult = await filesStorage.UploadAsync(memoryStream, request.ReactionImage.FileName, cancellationToken);

        if (!uploadResult.IsSuccess)
        {
            return ResultsHelper.BadRequest(uploadResult.GetValue<string>());
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

        var reactionType = new ReactionType
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            IconId = attachment.Id,
            Attachment = attachment
        };

        await reactions.AddAsync(reactionType, cancellationToken);
        await reactions.SaveChangesAsync(cancellationToken);

        return ResultsHelper.Created(reactionType.Id);
    }
}

public class CreateReactionRequestValidator : AbstractValidator<CreateReactionRequest>
{
    public CreateReactionRequestValidator()
    {
        RuleFor(x => x.ReactionImage).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
    }
}