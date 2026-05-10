namespace SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;

public static class OffsetPaginationExtensions
{
    /// <summary>
    /// Apply offset-based pagination (Skip/Take).
    /// </summary>
    public static IQueryable<T> ApplyOffsetPagination<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }

    /// <summary>
    /// Apply offset-based pagination using OffsetPagedQuery request.
    /// </summary>
    public static IQueryable<T> ApplyOffsetPagination<T>(
        this IQueryable<T> query,
        OffsetPagedQuery request)
    {
        return query.ApplyOffsetPagination(request.PageNumber, request.PageSize);
    }
}
