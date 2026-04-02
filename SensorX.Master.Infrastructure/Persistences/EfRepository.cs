using Ardalis.Specification.EntityFrameworkCore;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Infrastructure.Persistences;

public class EfRepository<T>(AppDbContext dbContext) : RepositoryBase<T>(dbContext), IRepository<T> where T : class, IAggregateRoot
{
    public async Task Add(T entity, CancellationToken cancellationToken) => await DbContext.Set<T>().AddAsync(entity, cancellationToken);
    public Task Update(T entity, CancellationToken cancellationToken)
    {
        DbContext.Set<T>().Update(entity);
        return Task.CompletedTask;
    }

    public Task Delete(T entity, CancellationToken cancellationToken)
    {
        DbContext.Set<T>().Remove(entity);
        return Task.CompletedTask;
    }

    public Task UpdateRange(IEnumerable<T> entities, CancellationToken cancellationToken)
    {
        DbContext.Set<T>().UpdateRange(entities);
        return Task.CompletedTask;
    }
}
