using Application.Abstractions.Persistence.Repositories.Media;
using Domain.Models.Messaging;

namespace Infrastructure.Persistence.Repositories.Media;

public class ReactionsTypesRepository : BaseRepository<ReactionType>, IReactionsTypesRepository
{
    public ReactionsTypesRepository(ChatsDbContext context) : base(context)
    {
    }
} 