using Application.Abstractions.Persistence.Repositories.Media;
using Domain.Models.Messaging;

namespace Infrastructure.Persistence.Repositories.Media;

public class ReactionsTypesRepository(ChatsDbContext context)
    : BaseRepository<ReactionType>(context), IReactionsTypesRepository;