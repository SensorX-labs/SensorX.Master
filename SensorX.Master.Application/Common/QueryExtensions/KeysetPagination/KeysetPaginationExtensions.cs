using System.Linq.Expressions;

namespace SensorX.Master.Application.Common.QueryExtensions.KeysetPagination;

public static class KeysetPaginationExtensions
{
    /// <summary>
    /// Apply keyset-based pagination.
    /// Uses CreatedAt + Id as composite cursor.
    ///
    /// Usage:
    /// var query = dbContext.Products.AsQueryable();
    ///
    /// query = query
    ///     .ApplyKeysetPagination(
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
    public static IQueryable<T> ApplyKeysetPagination<T, TId>(
        this IQueryable<T> query,
        KeysetPagedQuery request,
        Expression<Func<T, DateTimeOffset>> createdAtSelector,
        Expression<Func<T, TId>> idSelector)
    {
        var param = createdAtSelector.Parameters[0];

        // p.CreatedAt
        var createdAt = createdAtSelector.Body;

        // p.Id (normalize to same parameter "p")
        var id = ReplaceParameter(idSelector.Body, idSelector.Parameters[0], param);

        // Previous page
        if (request.IsPrevious && request.FirstCreatedAt.HasValue && request.FirstId.HasValue)
        {
            var firstId = CreateId<TId>(request.FirstId.Value);
            var predicate = BuildPrevious<T, TId>(
                param,
                createdAt,
                id,
                request.FirstCreatedAt.Value,
                firstId);

            return query.Where(predicate);
        }

        // Next page
        if (request.LastCreatedAt.HasValue && request.LastId.HasValue)
        {
            var lastId = CreateId<TId>(request.LastId.Value);
            var predicate = BuildNext<T, TId>(
                param,
                createdAt,
                id,
                request.LastCreatedAt.Value,
                lastId);

            return query.Where(predicate);
        }

        // First page (no keyset)
        return query;
    }

    private static TId CreateId<TId>(Guid guid)
    {
        if (typeof(TId) == typeof(Guid)) return (TId)(object)guid;
        // Hỗ trợ Strongly Typed ID kế thừa từ VoId(Guid Value)
        return (TId)Activator.CreateInstance(typeof(TId), guid)!;
    }

    /// <summary>
    /// Build predicate for previous page.
    /// </summary>
    private static Expression<Func<T, bool>> BuildPrevious<T, TId>(
        ParameterExpression param,
        Expression createdAt,
        Expression id,
        DateTimeOffset firstCreatedAt,
        TId firstId)
    {
        var body =
            Expression.OrElse(
                Expression.GreaterThan(createdAt, Expression.Constant(firstCreatedAt)),
                Expression.AndAlso(
                    Expression.Equal(createdAt, Expression.Constant(firstCreatedAt)),
                    Expression.GreaterThan(id, Expression.Constant(firstId, typeof(TId)))
                )
            );

        return Expression.Lambda<Func<T, bool>>(body, param);
    }

    /// <summary>
    /// Build predicate for next page.
    /// </summary>
    private static Expression<Func<T, bool>> BuildNext<T, TId>(
        ParameterExpression param,
        Expression createdAt,
        Expression id,
        DateTimeOffset lastCreatedAt,
        TId lastId)
    {
        var body =
            Expression.OrElse(
                Expression.LessThan(createdAt, Expression.Constant(lastCreatedAt)),
                Expression.AndAlso(
                    Expression.Equal(createdAt, Expression.Constant(lastCreatedAt)),
                    Expression.LessThan(id, Expression.Constant(lastId, typeof(TId)))
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