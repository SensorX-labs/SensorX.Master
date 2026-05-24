using MediatR;
using SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;

namespace SensorX.Master.Application.Queries.Quotes.GetPageListQuote;

public record GetPageListQuoteQuery(
    string? SearchTerm,
    QuoteStatus? Status,
    QuoteResponseStatus? ResponseType,
    bool? IsExpired
) : OffsetPagedQuery, IRequest<Result<OffsetPagedResult<GetPageListQuoteResponse>>>;

public record GetPageListQuoteResponse(
    Guid Id,
    string Code,
    QuoteStatus Status,
    DateTimeOffset? QuoteDate,
    Guid CustomerId,
    string CompanyName,
    decimal GrandTotal,
    int ItemCount,
    DateTimeOffset CreatedAt,
    QuoteResponseStatus? ResponseType
);

