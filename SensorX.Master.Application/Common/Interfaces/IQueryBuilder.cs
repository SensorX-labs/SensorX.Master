namespace SensorX.Master.Application.Common.Interfaces;

/// <summary>
/// QueryBuilder is responsible for exposing IQueryable sources.
/// It does NOT execute queries.
/// </summary>
public interface IQueryBuilder<T> where T : class
{
    /// <summary>
    /// Query with tracking (for update scenarios if needed)
    /// </summary>
    IQueryable<T> Query { get; }

    /// <summary>
    /// Query without tracking (recommended for read-only queries)
    /// </summary>
    IQueryable<T> QueryAsNoTracking { get; }
}