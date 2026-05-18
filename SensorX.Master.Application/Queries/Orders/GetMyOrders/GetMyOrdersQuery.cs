using MediatR;
using SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Application.Queries.Orders.GetPageListOrder;

namespace SensorX.Master.Application.Queries.Orders.GetMyOrders;

public record GetMyOrdersQuery : OffsetPagedQuery, IRequest<Result<OffsetPagedResult<GetPageListOrderResponse>>>
{
    public string? SearchTerm { get; init; }
}
