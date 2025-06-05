using System.Collections.Frozen;
using System.Runtime.CompilerServices;
using Application.Abstractions.Persistence.Repositories;
using Application.Dtos.Requests.Shared;
using Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class BaseRepository<T>(ChatsDbContext dbContext) : IBaseRepository<T>
    where T : class, IEntity
{
    protected readonly ChatsDbContext _dbContext = dbContext;

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
    public ValueTask<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<T>().FindAsync([id], cancellationToken: cancellationToken);
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
    public async ValueTask AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<T>().AddAsync(entity, cancellationToken);
    }

    public Task<List<T>> GetAllAsync(PageSettings requestPageSettings, CancellationToken cancellationToken)
    {
        return _dbContext.Set<T>().Skip(requestPageSettings.PageSize * (requestPageSettings.PageNumber - 1)).Take(requestPageSettings.PageSize).ToListAsync(cancellationToken: cancellationToken);
    }

    public void Delete(T entity)
    {
        _dbContext.Set<T>().Remove(entity);
    }

    public void Update(T entity)
    {
        _dbContext.Set<T>().Update(entity);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}