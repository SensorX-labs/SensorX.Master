using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;

namespace SensorX.Master.Application.Common.QueryExtensions.Search;

public static class QuoteSearch
{
    public static IQueryable<Quote> ApplySearch(
        this IQueryable<Quote> query,
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