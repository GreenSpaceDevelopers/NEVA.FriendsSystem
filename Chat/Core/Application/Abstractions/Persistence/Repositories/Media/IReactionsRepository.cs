using Application.Dtos.Requests.Shared;
using Domain.Models.Messaging;

namespace Application.Abstractions.Persistence.Repositories.Media;

public interface IReactionsRepository : IBaseRepository<Reaction>;

public interface IReactionsTypesRepository : IBaseRepository<ReactionType>;