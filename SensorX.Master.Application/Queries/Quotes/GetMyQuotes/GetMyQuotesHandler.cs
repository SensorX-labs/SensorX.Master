using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.QueryExtensions.LoadMore;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Application.Queries.Quotes.GetMyQuotes;

public class GetMyQuotesHandler(
    IQueryBuilder<Quote> _quoteBuilder,
    IQueryBuilder<Customer> _customerBuilder,
    IQueryExecutor _queryExecutor,
    ICurrentUser _currentUser
) : IRequestHandler<GetMyQuotesQuery, Result<GetMyQuotesResult>>
{
    public async Task<Result<GetMyQuotesResult>> Handle(GetMyQuotesQuery request, CancellationToken cancellationToken)
    {
        var customerQuery = _customerBuilder.QueryAsNoTracking
            .Where(c => c.AccountId == _currentUser.UserId)
            .Select(c => c.Id);
        var customerIdVal = await _queryExecutor.FirstOrDefaultAsync(customerQuery, cancellationToken);


        if (customerIdVal == null)
        {
            return Result<GetMyQuotesResult>.Failure("Khong tim thay khach hang cho tai khoan hien tai");
        }

        var customerId = new CustomerId(customerIdVal);

        var query = _quoteBuilder.QueryAsNoTracking.Where(quote => quote.CustomerId == customerId);


        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(quote => ((string)quote.Code).Contains(request.SearchTerm));
        }
        if (request.Status.HasValue)
        {
            query = query.Where(quote => quote.Status == request.Status);
        }


        var pageQuery = query.ApplyLoadMoreWithOrder(
            request.LastValue.ToCursor<DateTimeOffset>(),
            x => x.CreatedAt,
            request.LastId,
            x => x.Id,
            request.IsDescending
        );

        var pageSize = request.PageSize ?? 10;
        var items = await _queryExecutor.ToListAsync(pageQuery.Take(pageSize + 1), cancellationToken);

        bool hasNext = items.Count > pageSize;
        if (hasNext) items.RemoveAt(items.Count - 1);

        var responseItems = items.Select(x => new GetMyQuoteResponse(
            x.Id,
            x.Code,
            x.Status.ToString(),
            x.GetGrandTotal().Amount,
            x.CreatedAt
        )).ToList();

        var lastItem = responseItems.LastOrDefault();

        var result = new GetMyQuotesResult
        {
            Items = responseItems,
            LastId = lastItem?.Id,
            LastValue = lastItem?.CreatedAt.ToString("O"),
            HasNext = hasNext
        };

        return Result<GetMyQuotesResult>.Success(result);
    }
}
