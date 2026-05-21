namespace SensorX.Master.Application.Queries.Quotes.GetQuoteStats;

using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;

public sealed class GetQuoteStatsHandler(
    IQueryBuilder<Quote> _quoteBuilder,
    IQueryBuilder<SaleStaff> _saleStaffBulder,
    IQueryExecutor _queryExecutor,
    ICurrentUser _currentUser
) : IRequestHandler<GetQuoteStatsQuery, Result<QuoteStatsResponse>>
{
    public async Task<Result<QuoteStatsResponse>> Handle(GetQuoteStatsQuery request, CancellationToken cancellationToken)
    {
        var query = _quoteBuilder.QueryAsNoTracking;

        if (_currentUser.Role == Role.SaleStaff)
        {
            var staffId = await _queryExecutor.FirstOrDefaultAsync(
                _saleStaffBulder.QueryAsNoTracking
                    .Where(x => x.AccountId == _currentUser.UserId)
                    .Select(x => x.Id),
                cancellationToken
            );

            query = query.Where(x => x.SenderInfo.Id == staffId);
        }
        var totalCount = await _queryExecutor.CountAsync(query, cancellationToken);
        var draftCount = await _queryExecutor.CountAsync(query.Where(x => x.Status == QuoteStatus.Draft), cancellationToken);
        var pendingCount = await _queryExecutor.CountAsync(query.Where(x => x.Status == QuoteStatus.Pending), cancellationToken);
        var approvedCount = await _queryExecutor.CountAsync(query.Where(x => x.Status == QuoteStatus.Approved), cancellationToken);
        var returnedCount = await _queryExecutor.CountAsync(query.Where(x => x.Status == QuoteStatus.Returned), cancellationToken);
        var sentCount = await _queryExecutor.CountAsync(query.Where(x => x.Status == QuoteStatus.Sent), cancellationToken);
        var orderedCount = await _queryExecutor.CountAsync(query.Where(x => x.Status == QuoteStatus.Ordered), cancellationToken);
        var expiredCount = await _queryExecutor.CountAsync(
            query.Where(x => (x.QuoteDate > DateTimeOffset.UtcNow.AddDays(7)) &&
            x.Status != QuoteStatus.Ordered &&
            x.QuoteDate != null
        ), cancellationToken);

        return Result<QuoteStatsResponse>.Success(new QuoteStatsResponse
        {
            TotalCount = totalCount,
            DraftCount = draftCount,
            PendingCount = pendingCount,
            ApprovedCount = approvedCount,
            ReturnedCount = returnedCount,
            SentCount = sentCount,
            OrderedCount = orderedCount,
            ExpiredCount = expiredCount
        });
    }
}