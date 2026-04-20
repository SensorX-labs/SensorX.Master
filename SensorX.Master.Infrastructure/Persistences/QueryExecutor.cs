using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SensorX.Master.Application.Common.Interfaces;

namespace SensorX.Master.Infrastructure.Persistences;

/// <summary>
/// EF Core implementation of IQueryExecutor.
/// Responsible for executing IQueryable queries only.
/// </summary>
public class QueryExecutor : IQueryExecutor
{
    // =========================
    // MATERIALIZATION
    // =========================

    public Task<List<T>> ToListAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default)
    {
        return query.ToListAsync(cancellationToken);
    }

    public Task<T?> FirstOrDefaultAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default)
    {
        return query.FirstOrDefaultAsync(cancellationToken);
    }

    public Task<T> FirstAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default)
    {
        return query.FirstAsync(cancellationToken);
    }

    public Task<T?> SingleOrDefaultAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default)
    {
        return query.SingleOrDefaultAsync(cancellationToken);
    }

    public Task<T> SingleAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default)
    {
        return query.SingleAsync(cancellationToken);
    }

    // =========================
    // AGGREGATION
    // =========================

    public Task<int> CountAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default)
    {
        return query.CountAsync(cancellationToken);
    }

    public Task<bool> AnyAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default)
    {
        return query.AnyAsync(cancellationToken);
    }

    public Task<bool> AllAsync<T>(
        IQueryable<T> query,
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return query.AllAsync(predicate, cancellationToken);
    }
}