using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;
using SensorX.Master.Application.Common.QueryExtensions.Search;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;

namespace SensorX.Master.Application.Queries.Quotes.GetPageListQuote;

public class GetPageListQuoteHandler(
    IQueryBuilder<Quote> _quoteQueryBuilder,
    IQueryExecutor _queryExecutor
) : IRequestHandler<GetPageListQuoteQuery, Result<QuoteOffsetPagedResult>>
{
    public async Task<Result<QuoteOffsetPagedResult>> Handle(
        GetPageListQuoteQuery request,
        CancellationToken cancellationToken)
    {
        var sourceQuery = _quoteQueryBuilder.QueryAsNoTracking.ApplySearch(request.SearchTerm);
        var totalCount = await _queryExecutor.CountAsync(sourceQuery, cancellationToken);

        var pagedQuery = sourceQuery
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .ApplyOffsetPagination(request);

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

        var items = await _queryExecutor.ToListAsync(dtoQuery, cancellationToken);

        var result = new QuoteOffsetPagedResult
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result<QuoteOffsetPagedResult>.Success(result);
    }
}
