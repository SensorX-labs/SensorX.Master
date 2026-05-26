using MediatR;
using SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.Orders.GetPageListOrder;

public record GetPageListOrderQuery : OffsetPagedQuery, IRequest<Result<OffsetPagedResult<GetPageListOrderResponse>>>
{
    public string? SearchTerm { get; init; }
    public string? Status { get; init; }
    public string? Code { get; init; }
    public string? CompanyName { get; init; }
    public string? RecipientName { get; init; }
    public string? RecipientPhone { get; init; }
    public string? SenderName { get; init; }
    public decimal? TotalFrom { get; init; }
    public decimal? TotalTo { get; init; }
    public DateTimeOffset? OrderDateFrom { get; init; }
    public DateTimeOffset? OrderDateTo { get; init; }
    public DateTimeOffset? CreatedFrom { get; init; }
    public DateTimeOffset? CreatedTo { get; init; }
}
