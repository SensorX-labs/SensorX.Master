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

        // CustomerInfo là owned type với cột string thuần túy -> EF Core dịch được
        // Không dùng Code.Value vì Code là Value Object qua HasConversion -> không dịch được
        return query.Where(p =>
            p.CustomerInfo.CompanyName.ToLower().Contains(term)
        );
    }
}