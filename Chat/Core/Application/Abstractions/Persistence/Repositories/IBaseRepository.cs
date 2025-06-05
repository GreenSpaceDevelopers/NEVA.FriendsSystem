using System.Collections.Frozen;
using Application.Dtos.Requests.Shared;
using Domain.Abstractions;

namespace Application.Abstractions.Persistence.Repositories;

public interface IBaseRepository<T> where T : class, IEntity
{
    public ValueTask<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    public ValueTask AddAsync(T entity, CancellationToken cancellationToken = default);
    public Task<List<T>> GetAllAsync(PageSettings requestPageSettings, CancellationToken cancellationToken);
    public void Delete(T entity);
    public void Update(T entity);
    public Task SaveChangesAsync(CancellationToken cancellationToken = default);
}