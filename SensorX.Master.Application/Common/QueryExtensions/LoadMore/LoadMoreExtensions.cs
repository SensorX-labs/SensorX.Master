using System.Linq.Expressions;

namespace SensorX.Master.Application.Common.QueryExtensions.LoadMore;

public static class LoadMoreExtensions
{
    /// <summary>
    /// Apply keyset-based pagination (Load More) using a generic key + Id (Guid).
    /// </summary>
    public static IQueryable<T> ApplyLoadMore<T, TKey>(
        this IQueryable<T> query,
        TKey? currentLastValue,
        Expression<Func<T, TKey>> keySelector,
        Guid? lastId,
        Expression<Func<T, Guid>> idSelector,
        bool isDescending = true)
    {
        // Kiểm tra an toàn: Nếu thiếu một trong hai cursor thì không phân trang (lấy trang đầu)
        if (currentLastValue == null || lastId == null)
            return query;

        var param = keySelector.Parameters[0];
        var keyExpr = keySelector.Body;
        var idExpr = ReplaceParameter(idSelector.Body, idSelector.Parameters[0], param);

        var lastValueConstant = Expression.Constant(currentLastValue, typeof(TKey));
        var lastIdConstant = Expression.Constant(lastId.Value, typeof(Guid));

        Expression keyCompare;
        Expression idCompare;

        if (isDescending)
        {
            keyCompare = Expression.LessThan(keyExpr, lastValueConstant);
            idCompare = Expression.LessThan(idExpr, lastIdConstant);
        }
        else
        {
            keyCompare = Expression.GreaterThan(keyExpr, lastValueConstant);
            idCompare = Expression.GreaterThan(idExpr, lastIdConstant);
        }

        var body = Expression.OrElse(
            keyCompare,
            Expression.AndAlso(
                Expression.Equal(keyExpr, lastValueConstant),
                idCompare
            )
        );

        var predicate = Expression.Lambda<Func<T, bool>>(body, param);
        return query.Where(predicate);
    }

    /// <summary>
    /// Applies both Load More filtering and the corresponding OrderBy/ThenBy sorting.
    /// </summary>
    public static IQueryable<T> ApplyLoadMoreWithOrder<T, TKey>(
        this IQueryable<T> query,
        TKey? currentLastValue,
        Expression<Func<T, TKey>> keySelector,
        Guid? lastId,
        Expression<Func<T, Guid>> idSelector,
        bool isDescending = true)
    {
        var filteredQuery = query.ApplyLoadMore(currentLastValue, keySelector, lastId, idSelector, isDescending);

        return isDescending
            ? filteredQuery.OrderByDescending(keySelector).ThenByDescending(idSelector)
            : filteredQuery.OrderBy(keySelector).ThenBy(idSelector);
    }

    /// <summary>
    /// Converts a string cursor value to the specified type T.
    /// </summary>
    public static T? ToCursor<T>(this string? value) where T : struct
    {
        if (string.IsNullOrEmpty(value)) return null;

        try
        {
            var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));
            return (T?)converter.ConvertFromString(value);
        }
        catch
        {
            return null;
        }
    }

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
