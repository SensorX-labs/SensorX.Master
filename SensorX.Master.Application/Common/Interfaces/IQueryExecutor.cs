using System.Linq.Expressions;

namespace SensorX.Master.Application.Common.Interfaces;

/// <summary>
/// QueryExecutor is responsible for executing IQueryable expressions.
/// It abstracts EF Core (or any ORM) away from Application layer.
/// 
/// Responsibilities:
/// - Execute LINQ queries
/// - Return materialized results
/// - Perform aggregation operations
/// 
/// NOT responsible for:
/// - Building queries
/// - Filtering / sorting / joining logic
/// </summary>
public interface IQueryExecutor
{
    // =========================
    // MATERIALIZATION OPERATIONS
    // =========================

    /// <summary>
    /// Executes query and returns all results as a list.
    /// Equivalent to: ToListAsync() in EF Core.
    /// </summary>
    Task<List<T>> ToListAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the first element of a sequence or null if empty.
    /// Equivalent to: FirstOrDefaultAsync()
    /// </summary>
    Task<T?> FirstOrDefaultAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the first element of a sequence.
    /// Throws exception if sequence is empty.
    /// Equivalent to: FirstAsync()
    /// </summary>
    Task<T> FirstAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a single element or null.
    /// Throws if more than one element exists.
    /// Equivalent to: SingleOrDefaultAsync()
    /// </summary>
    Task<T?> SingleOrDefaultAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns exactly one element.
    /// Throws if 0 or more than 1 elements exist.
    /// Equivalent to: SingleAsync()
    /// </summary>
    Task<T> SingleAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default);

    // =========================
    // AGGREGATION OPERATIONS
    // =========================

    /// <summary>
    /// Counts number of elements in query.
    /// Equivalent to: COUNT(*) in SQL.
    /// </summary>
    Task<int> CountAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any element exists in query.
    /// Equivalent to: EXISTS in SQL.
    /// </summary>
    Task<bool> AnyAsync<T>(
        IQueryable<T> query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if all elements satisfy a condition.
    /// Equivalent to: ALL in LINQ / NOT EXISTS failure in SQL.
    /// </summary>
    Task<bool> AllAsync<T>(
        IQueryable<T> query,
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);
}