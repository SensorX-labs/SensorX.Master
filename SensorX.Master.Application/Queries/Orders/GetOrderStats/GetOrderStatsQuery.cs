using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.Orders.GetOrderStats;

public record GetOrderStatsQuery : IRequest<Result<GetOrderStatsResponse>>;

public class GetOrderStatsResponse
{
    public int TotalCount { get; init; }
    public int PendingPaymentCount { get; init; }
    public int ProcessingCount { get; init; }
    public int DispatchedCount { get; init; }
    public int CancelledCount { get; init; }
}
