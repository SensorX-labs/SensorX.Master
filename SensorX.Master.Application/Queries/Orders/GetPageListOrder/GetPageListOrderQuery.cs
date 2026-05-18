using MediatR;
using SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.Orders.GetPageListOrder;

public record GetPageListOrderQuery : OffsetPagedQuery, IRequest<Result<OffsetPagedResult<GetPageListOrderResponse>>>
{
    public string? SearchTerm { get; init; }
}
