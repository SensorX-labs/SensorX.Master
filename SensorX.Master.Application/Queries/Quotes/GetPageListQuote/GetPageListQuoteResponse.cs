using SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;

namespace SensorX.Master.Application.Queries.Quotes.GetPageListQuote;

public record GetPageListQuoteResponse(
    Guid Id,
    string Code,
    string Status,
    DateTimeOffset QuoteDate,
    Guid CustomerId,
    string RecipientName,
    string CompanyName,
    decimal GrandTotal,
    int ItemCount,
    DateTimeOffset CreatedAt
);


public class QuoteOffsetPagedResult : OffsetPagedResult<GetPageListQuoteResponse> { }