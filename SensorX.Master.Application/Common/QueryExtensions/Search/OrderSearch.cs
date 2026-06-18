using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;

namespace SensorX.Master.Application.Common.QueryExtensions.Search;

public static class OrderSearch
{
    public static IQueryable<Order> ApplySearch(
        this IQueryable<Order> query,
        string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var term = searchTerm.Trim().ToLower();

        return query.Where(o => o.Code.Value.ToLower().Contains(term));
    }
}
