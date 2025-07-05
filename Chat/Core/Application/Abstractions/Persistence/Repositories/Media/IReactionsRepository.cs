using Application.Dtos.Requests.Shared;
using Domain.Models.Messaging;

namespace Application.Abstractions.Persistence.Repositories.Media;

public interface IReactionsRepository : IBaseRepository<MessageReaction>;

public interface IReactionsTypesRepository : IBaseRepository<ReactionType>;