using MediatR;
using SensorX.Master.Application.Common.QueryExtensions.LoadMore;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;

namespace SensorX.Master.Application.Queries.Quotes.GetMyQuotes;

public sealed record GetMyQuotesQuery(
    string? SearchTerm,
    QuoteStatus? Status
) : LoadMoreQuery, IRequest<Result<GetMyQuotesResult>>;

public sealed record GetMyQuoteResponse(
    Guid Id,
    string Code,
    string Status,
    decimal TotalAmount,
    DateTimeOffset CreatedAt
);

public sealed class GetMyQuotesResult : LoadMoreResult<GetMyQuoteResponse>;
