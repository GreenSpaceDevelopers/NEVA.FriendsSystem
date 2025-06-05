using Application.Abstractions.Persistence.Repositories.Media;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Domain.Models.Messaging;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Application.Requests.Commands;

public record CreateStickerRequest(IFormFile stickerImage, string name) : IRequest;

public class CreateStickerRequestHandler(
    IFilesStorage filesStorage,
    IFilesValidator filesValidator,
    IAttachmentsRepository attachments) : IRequestHandler<CreateStickerRequest>
{
    public async Task<IOperationResult> HandleAsync(CreateStickerRequest request, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await request.stickerImage.CopyToAsync(memoryStream, cancellationToken);

        if (filesValidator.ValidateFile(memoryStream, "sticker") is not true)
        {
            return ResultsHelper.BadRequest("Invalid file");
        }

        var uploadResult = await filesStorage.UploadAsync(memoryStream, "sticker", cancellationToken);

        if (!uploadResult.IsSuccess)
        {
            return ResultsHelper.BadRequest(uploadResult.GetValue<string>());
        }

        var type = await attachments.GetAttachmentTypeAsync(AttachmentTypes.Sticker, cancellationToken);

        var attachment = new Attachment
        {
            Id = Guid.NewGuid(),
            Url = uploadResult.GetValue<string>(),
            Type = type,
            TypeId = type.Id,
        };

        await attachments.AddAsync(attachment, cancellationToken);
        await attachments.SaveChangesAsync(cancellationToken);
        
        return ResultsHelper.Created(attachment.Id);
    }
}

public class CreateStickerRequestValidator : AbstractValidator<CreateStickerRequest>
{
    public CreateStickerRequestValidator()
    {
        RuleFor(x => x.stickerImage).NotEmpty();
    }
}