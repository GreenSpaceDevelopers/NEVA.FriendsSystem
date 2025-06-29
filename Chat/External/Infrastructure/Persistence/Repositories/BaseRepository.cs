using Application.Abstractions.Persistence.Repositories;
using Application.Dtos.Requests.Shared;
using Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class BaseRepository<T>(ChatsDbContext dbContext) : IBaseRepository<T>
    where T : class, IEntity
{
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<T>().FindAsync([id], cancellationToken: cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<T>().AddAsync(entity, cancellationToken);
    }

    public async Task<List<T>> GetAllAsync(PageSettings requestPageSettings, CancellationToken cancellationToken)
    {
        return await dbContext.Set<T>()
            .Skip(requestPageSettings.Skip)
            .Take(requestPageSettings.Take)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public void Delete(T entity)
    {
        dbContext.Set<T>().Remove(entity);
    }

    public void Update(T entity)
    {
        dbContext.Set<T>().Update(entity);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}