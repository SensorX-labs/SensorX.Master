using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Application.Queries.RFQs.GetStats;

public sealed class GetStatsHandler(
    IQueryBuilder<RFQ> _rfqQueryBuilder,
    IQueryBuilder<SaleStaff> _staffQueryBuilder,
    IQueryExecutor _queryExecutor,
    ICurrentUser _currentUser
) : IRequestHandler<GetStatsQuery, Result<GetStatsResponse>>
{
    public async Task<Result<GetStatsResponse>> Handle(GetStatsQuery request, CancellationToken cancellationToken)
    {
        var query = _rfqQueryBuilder.QueryAsNoTracking;
        if (_currentUser.Role == Role.SaleStaff)
        {
            var accountId = new AccountId(_currentUser.UserId ?? Guid.Empty);
            var staff = await _queryExecutor.FirstOrDefaultAsync(
                _staffQueryBuilder.QueryAsNoTracking.Where(x => x.AccountId == accountId),
                cancellationToken
            );
            if (staff is not null)
            {
                query = query.Where(x => x.StaffId == staff.Id);
            }
        }

        var totalCount = await _queryExecutor.CountAsync(query, cancellationToken);
        var pendingCount = await _queryExecutor.CountAsync(query.Where(x => x.Status == RFQStatus.Pending), cancellationToken);
        var acceptedCount = await _queryExecutor.CountAsync(query.Where(x => x.Status == RFQStatus.Accepted), cancellationToken);
        var rejectedCount = await _queryExecutor.CountAsync(query.Where(x => x.Status == RFQStatus.Rejected), cancellationToken);
        var respondedCount = await _queryExecutor.CountAsync(query.Where(x => x.Status == RFQStatus.Responded), cancellationToken);
        var convertedCount = await _queryExecutor.CountAsync(query.Where(x => x.Status == RFQStatus.Converted), cancellationToken);

        return Result<GetStatsResponse>.Success(new GetStatsResponse(
            totalCount,
            pendingCount,
            acceptedCount,
            rejectedCount,
            respondedCount,
            convertedCount
        ));
    }
}