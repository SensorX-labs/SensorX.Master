using MediatR;
using SensorX.Master.Application.Common.QueryExtensions.LoadMore;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;

namespace SensorX.Master.Application.Queries.Quotes.GetMyQuotes;

public sealed record GetMyQuotesQuery(
    string? SearchTerm,
    StatusCustomerCanSeeQuote? Status
) : LoadMoreQuery, IRequest<Result<GetMyQuotesResult>>;

public enum StatusCustomerCanSeeQuote
{
    Pending,
    Accepted,
    Declined,
    Expired
}

public sealed record GetMyQuoteResponse(
    Guid Id,
    string Code,
    StatusCustomerCanSeeQuote Status,
    decimal TotalAmount,
    DateTimeOffset CreatedAt
);

public sealed class GetMyQuotesResult : LoadMoreResult<GetMyQuoteResponse>;
