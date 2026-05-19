using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.QueryExtensions.LoadMore;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;

namespace SensorX.Master.Application.Queries.RFQs.GetMyRFQPage;

public class GetMyRFQPageHandler(
    IQueryBuilder<RFQ> _rfqBuilder,
    IQueryBuilder<Customer> _customerBuilder,
    IQueryExecutor _queryExecutor,
    ICurrentUser _currentUser
) : IRequestHandler<GetMyRFQPageQuery, Result<GetMyRFQResult>>
{
    public async Task<Result<GetMyRFQResult>> Handle(GetMyRFQPageQuery request, CancellationToken cancellationToken)
    {
        var customerQuery = _customerBuilder.QueryAsNoTracking
            .Where(c => c.AccountId == _currentUser.UserId)
            .Select(c => c.Id);
        var customerId = await _queryExecutor.FirstOrDefaultAsync(customerQuery, cancellationToken);

        var query = _rfqBuilder.QueryAsNoTracking.Where(rfq => rfq.CustomerId == customerId);
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(rfq => ((string)rfq.Code).Contains(request.SearchTerm));
        }
        if (request.Status.HasValue)
        {
            query = query.Where(rfq => rfq.Status == request.Status);
        }
        var pageQuery = query.ApplyLoadMoreWithOrder(
            request.LastValue.ToCursor<DateTimeOffset>(),
            x => x.CreatedAt,
            request.LastId,
            x => (Guid)x.Id,
            request.IsDescending
        );

        var pageSize = request.PageSize ?? 10;
        var items = await _queryExecutor.ToListAsync(pageQuery.Take(pageSize + 1), cancellationToken);

        bool hasNext = items.Count > pageSize;
        if (hasNext) items.RemoveAt(items.Count - 1);

        var responseItems = items.Select(x => new GetMyRFQResponse(
            (Guid)x.Id,
            x.Code,
            x.Status,
            x.CreatedAt
        )).ToList();

        var lastItem = responseItems.LastOrDefault();

        var result = new GetMyRFQResult
        {
            Items = responseItems,
            LastId = lastItem?.Id,
            LastValue = lastItem?.CreatedAt.ToString("O"),
            HasNext = hasNext
        };

        return Result<GetMyRFQResult>.Success(result);
    }
}