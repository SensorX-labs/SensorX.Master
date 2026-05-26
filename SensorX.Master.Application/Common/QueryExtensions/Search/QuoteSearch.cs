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

        var term = searchTerm.Trim().ToLower();

        return query.Where(p =>
            ((string)p.Code).ToLower().Contains(term) ||
            p.CustomerInfo.CompanyName.ToLower().Contains(term) ||
            ((string)p.CustomerInfo.Email).ToLower().Contains(term) ||
            ((string)p.CustomerInfo.Phone).ToLower().Contains(term) ||
            p.SenderInfo.Name.ToLower().Contains(term)
        );
    }
}
