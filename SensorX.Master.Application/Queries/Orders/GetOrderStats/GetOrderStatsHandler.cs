using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;

namespace SensorX.Master.Application.Queries.Orders.GetOrderStats;

public class GetOrderStatsHandler(
    IQueryBuilder<Order> orderQueryBuilder,
    IQueryExecutor queryExecutor
) : IRequestHandler<GetOrderStatsQuery, Result<GetOrderStatsResponse>>
{
    public async Task<Result<GetOrderStatsResponse>> Handle(GetOrderStatsQuery request, CancellationToken cancellationToken)
    {
        var query = orderQueryBuilder.QueryAsNoTracking;

        var totalCount = await queryExecutor.CountAsync(query, cancellationToken);
        var pendingPaymentCount = await queryExecutor.CountAsync(query.Where(x => x.Status == OrderStatus.PendingPayment), cancellationToken);
        var processingCount = await queryExecutor.CountAsync(query.Where(x => x.Status == OrderStatus.Processing), cancellationToken);
        var dispatchedCount = await queryExecutor.CountAsync(query.Where(x => x.Status == OrderStatus.Dispatched), cancellationToken);
        var cancelledCount = await queryExecutor.CountAsync(query.Where(x => x.Status == OrderStatus.Cancelled), cancellationToken);

        return Result<GetOrderStatsResponse>.Success(new GetOrderStatsResponse
        {
            TotalCount = totalCount,
            PendingPaymentCount = pendingPaymentCount,
            ProcessingCount = processingCount,
            DispatchedCount = dispatchedCount,
            CancelledCount = cancelledCount
        });
    }
}
