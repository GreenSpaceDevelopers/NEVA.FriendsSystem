using Application.Abstractions.Persistence.Repositories.Media;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using FluentValidation;

namespace Application.Requests.Commands;

public record DeleteStickerRequest(Guid StickerId) : IRequest;

public class DeleteStickerRequestHandler(IAttachmentsRepository attachments, IFilesStorage filesStorage) : IRequestHandler<DeleteStickerRequest>
{
    public async Task<IOperationResult> HandleAsync(DeleteStickerRequest request, CancellationToken cancellationToken = default)
    {
        var sticker = await attachments.GetByIdAsync(request.StickerId, cancellationToken);

        if (sticker is null)
        {
            return ResultsHelper.NotFound("Sticker not found");
        }

        if (!string.IsNullOrEmpty(sticker.Url))
        {
            await filesStorage.DeleteAsync(sticker.Url, cancellationToken);
        }

        attachments.Delete(sticker);
        await attachments.SaveChangesAsync(cancellationToken);

        return ResultsHelper.NoContent();
    }
}

public class DeleteStickerRequestValidator : AbstractValidator<DeleteStickerRequest>
{
    public DeleteStickerRequestValidator()
    {
        RuleFor(x => x.StickerId).NotEmpty();
    }
}