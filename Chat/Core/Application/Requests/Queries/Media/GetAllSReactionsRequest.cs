using Application.Abstractions.Persistence.Repositories.Media;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Common.Mappers;
using Application.Dtos;
using Application.Dtos.Requests.Shared;

namespace Application.Requests.Queries.Media;

public record GetAllSReactionsRequest(PageSettings PageSettings) : IRequest;

public record GetAllSReactionsRequestHandler
    (IReactionsTypesRepository reactionsTypesRepository,
        IFilesSigningService filesSigningService,
        IAttachmentsRepository attachments) : IRequestHandler<GetAllSReactionsRequest>
{
    public async Task<IOperationResult> HandleAsync(GetAllSReactionsRequest request, CancellationToken cancellationToken = default)
    {
        var reactionTypes = await reactionsTypesRepository.GetAllAsync(request.PageSettings, cancellationToken);

        var mappedDtos = new List<MediaDto>();

        await Parallel.ForEachAsync(reactionTypes, cancellationToken, async (reactionType, token) =>
        {
            var attachment = await attachments.GetByIdAsync(reactionType.IconId, token);
            
            if (attachment is null)
            {
                return;
            }
            
            var signedUrl = await filesSigningService.GetSignedUrlAsync(attachment.Url, token);
            
            mappedDtos.Add(reactionType.ToDto(signedUrl));
        });
        
        return ResultsHelper.Ok(mappedDtos);
    }
}