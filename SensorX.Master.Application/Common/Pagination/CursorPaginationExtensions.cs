using System.Linq.Expressions;

namespace SensorX.Master.Application.Common.Pagination;

public static class CursorPaginationExtensions
{
    /// <summary>
    /// Apply cursor-based pagination (keyset pagination).
    /// Uses CreatedAt + Id as composite cursor.
    ///
    /// Usage:
    /// var query = dbContext.Products.AsQueryable();
    ///
    /// query = query
    ///     .ApplyCursorPagination(
    ///         request,
    ///         p => p.CreatedAt,
    ///         p => p.Id)
    ///     .OrderByDescending(p => p.CreatedAt)
    ///     .ThenByDescending(p => p.Id);
    ///
    /// var items = await query.Take(request.PageSize + 1).ToListAsync();
    ///
    /// Notes:
    /// - Always apply OrderBy AFTER this method
    /// - Use CreatedAt + Id to avoid duplicate records
    /// - Take(PageSize + 1) to determine HasNext
    /// </summary>
    public static IQueryable<T> ApplyCursorPagination<T>(
        this IQueryable<T> query,
        CursorPagedQuery request,
        Expression<Func<T, DateTimeOffset>> createdAtSelector,
        Expression<Func<T, Guid>> idSelector)
    {
        var param = createdAtSelector.Parameters[0];

        // p.CreatedAt
        var createdAt = createdAtSelector.Body;

        // p.Id (normalize to same parameter "p")
        var id = ReplaceParameter(idSelector.Body, idSelector.Parameters[0], param);

        // Previous page
        if (request.IsPrevious && request.FirstCreatedAt.HasValue && request.FirstId.HasValue)
        {
            var predicate = BuildPrevious<T>(
                param,
                createdAt,
                id,
                request.FirstCreatedAt.Value,
                request.FirstId.Value);

            return query.Where(predicate);
        }

        // Next page
        if (request.LastCreatedAt.HasValue && request.LastId.HasValue)
        {
            var predicate = BuildNext<T>(
                param,
                createdAt,
                id,
                request.LastCreatedAt.Value,
                request.LastId.Value);

            return query.Where(predicate);
        }

        // First page (no cursor)
        return query;
    }

    // Expression helper mapping:
    // OrElse      → ||
    // AndAlso     → &&
    // GreaterThan → >
    // LessThan    → <
    // Equal       → ==
    // Constant    → value
    // Lambda      → x => ...

    /// <summary>
    /// Build predicate for previous page.
    /// </summary>
    private static Expression<Func<T, bool>> BuildPrevious<T>(
        ParameterExpression param,
        Expression createdAt,
        Expression id,
        DateTimeOffset firstCreatedAt,
        Guid firstId)
    {
        var body =
            Expression.OrElse(
                Expression.GreaterThan(createdAt, Expression.Constant(firstCreatedAt)),
                Expression.AndAlso(
                    Expression.Equal(createdAt, Expression.Constant(firstCreatedAt)),
                    Expression.GreaterThan(id, Expression.Constant(firstId))
                )
            );

        return Expression.Lambda<Func<T, bool>>(body, param);
    }

    /// <summary>
    /// Build predicate for next page.
    /// </summary>
    private static Expression<Func<T, bool>> BuildNext<T>(
        ParameterExpression param,
        Expression createdAt,
        Expression id,
        DateTimeOffset lastCreatedAt,
        Guid lastId)
    {
        var body =
            Expression.OrElse(
                Expression.LessThan(createdAt, Expression.Constant(lastCreatedAt)),
                Expression.AndAlso(
                    Expression.Equal(createdAt, Expression.Constant(lastCreatedAt)),
                    Expression.LessThan(id, Expression.Constant(lastId))
                )
            );

        return Expression.Lambda<Func<T, bool>>(body, param);
    }

    /// <summary>
    /// Replace parameter so both expressions share the same variable (p).
    /// </summary>
    private static Expression ReplaceParameter(
        Expression body,
        ParameterExpression oldParam,
        ParameterExpression newParam)
    {
        return new ReplaceVisitor(oldParam, newParam).Visit(body);
    }

    private class ReplaceVisitor(
        ParameterExpression oldParam,
        ParameterExpression newParam
    ) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == oldParam ? newParam : base.VisitParameter(node);
    }
}