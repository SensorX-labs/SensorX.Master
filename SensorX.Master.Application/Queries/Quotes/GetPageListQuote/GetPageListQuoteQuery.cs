using MediatR;
using SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.Quotes.GetPageListQuote;

public record GetPageListQuoteQuery(
    string? SearchTerm
) : OffsetPagedQuery, IRequest<Result<OffsetPagedResult<GetPageListQuoteResponse>>>;

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

