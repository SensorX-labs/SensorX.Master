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

        var term = searchTerm.Trim().ToLower();

        return query.Where(p =>
            ((string)p.Code).ToLower().Contains(term) ||
            p.CustomerInfo!.CompanyName.ToLower().Contains(term) ||
            ((string)p.CustomerInfo.Phone).ToLower().Contains(term)
        );
    }
}
