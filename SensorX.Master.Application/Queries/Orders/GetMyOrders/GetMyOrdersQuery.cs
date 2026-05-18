using MediatR;
using SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Application.Queries.Orders.GetPageListOrder;

namespace SensorX.Master.Application.Queries.Orders.GetMyOrders;

public record GetMyOrdersQuery(
    string? SearchTerm,
    int PageNumber = 1,
    int PageSize = 10
) : OffsetPagedQuery(PageNumber, PageSize), IRequest<Result<OffsetPagedResult<GetPageListOrderResponse>>>;
