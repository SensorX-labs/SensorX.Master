using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.Pagination;
using SensorX.Master.Application.Common.QueryExtensions.Search;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Queries.Quotes.GetPageListQuote;

public class GetPageListQuoteHandler(
    IQueryBuilder<Quote> _quoteQueryBuilder,
    IQueryExecutor _queryExecutor
) : IRequestHandler<GetPageListQuoteQuery, Result<QuoteCursorPagedResult>>
{
    public async Task<Result<QuoteCursorPagedResult>> Handle(
        GetPageListQuoteQuery request,
        CancellationToken cancellationToken)
    {
        var sourceQuery = _quoteQueryBuilder.QueryAsNoTracking.ApplySearch(request.SearchTerm);
        var pagedQuery = sourceQuery.ApplyCursorPagination(
            request,
            x => x.CreatedAt,
            x => x.Id.Value
        )
        .OrderByDescending(x => x.CreatedAt)
        .ThenByDescending(x => x.Id.Value);

        var dtoQuery = pagedQuery.Select(x => new GetPageListQuoteResponse(
            x.Id.Value,
            x.Code.Value,
            x.Status.ToString(),
            x.QuoteDate,
            x.CustomerId.Value,
            x.CustomerInfo.RecipientName,
            x.CustomerInfo.CompanyName,
            x.GetGrandTotal().Amount,
            x.LineItems.Count,
            x.CreatedAt
        ));

        var items = await _queryExecutor.ToListAsync(dtoQuery
            .Take(request.PageSize + 1), cancellationToken);

        var hasNext = items.Count > request.PageSize;
        if (hasNext) items.RemoveAt(request.PageSize);

        var result = new QuoteCursorPagedResult
        {
            Items = items,
            HasNext = hasNext,
            HasPrevious = request.IsPrevious,
            FirstCreatedAt = items.FirstOrDefault()?.CreatedAt,
            FirstId = items.FirstOrDefault()?.Id,
            LastCreatedAt = items.LastOrDefault()?.CreatedAt,
            LastId = items.LastOrDefault()?.Id
        };

        return Result<QuoteCursorPagedResult>.Success(result);
    }
}
