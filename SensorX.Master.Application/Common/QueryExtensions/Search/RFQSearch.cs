using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;

namespace SensorX.Master.Application.Common.QueryExtensions.Search;

public static class RFQSearch
{
    public static IQueryable<RFQ> ApplySearch(
        this IQueryable<RFQ> query,
        string? searchTerm
    )
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var term = searchTerm.Trim();

        return query.Where(p =>
            p.Code.Value.StartsWith(term) ||
            p.CustomerInfo.CompanyName.StartsWith(term)
        );
    }
}