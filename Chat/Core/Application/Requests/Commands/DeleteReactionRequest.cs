using Application.Abstractions.Persistence.Repositories.Media;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using FluentValidation;

namespace Application.Requests.Commands;

public record DeleteReactionRequest(Guid Id) : IRequest;

public class DeleteReactionRequestHandler(IReactionsTypesRepository reactionsTypesRepository) : IRequestHandler<DeleteReactionRequest>
{
    public async Task<IOperationResult> HandleAsync(DeleteReactionRequest request, CancellationToken cancellationToken = default)
    {
        var reactionType = await reactionsTypesRepository.GetByIdAsync(request.Id, cancellationToken);

        if (reactionType is null)
        {
            return ResultsHelper.NotFound("Reaction type not found");
        }

        reactionsTypesRepository.Delete(reactionType);
        await reactionsTypesRepository.SaveChangesAsync(cancellationToken);
        return ResultsHelper.NoContent();
    }
}

public class DeleteReactionRequestValidator : AbstractValidator<DeleteReactionRequest>
{
    public DeleteReactionRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}