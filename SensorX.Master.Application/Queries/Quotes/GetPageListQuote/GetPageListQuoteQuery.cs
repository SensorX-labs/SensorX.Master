using MediatR;
using SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;

namespace SensorX.Master.Application.Queries.Quotes.GetPageListQuote;

public record GetPageListQuoteQuery(
    string? SearchTerm,
    QuoteStatus? Status,
    QuoteResponseStatus? ResponseType,
    bool? IsExpired,
    string? Code,
    string? CompanyName,
    string? CustomerEmail,
    string? CustomerPhone,
    string? SenderName,
    decimal? TotalFrom,
    decimal? TotalTo,
    DateTimeOffset? QuoteDateFrom,
    DateTimeOffset? QuoteDateTo,
    DateTimeOffset? CreatedFrom,
    DateTimeOffset? CreatedTo
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

