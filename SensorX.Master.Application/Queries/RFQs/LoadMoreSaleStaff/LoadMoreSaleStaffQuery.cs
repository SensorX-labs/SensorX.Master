using MediatR;
using SensorX.Master.Application.Common.QueryExtensions.LoadMore;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.RFQs.LoadMoreSaleStaff;

public sealed record LoadMoreSaleStaffQuery : LoadMoreQuery, IRequest<Result<LoadMoreSaleStaffResult>>
{
    public string? SearchTerm { get; init; }
}

public sealed record LoadMoreSaleStaffResponse(
    Guid Id,
    string Code,
    string Name,
    string Phone,
    string Email
);

public sealed class LoadMoreSaleStaffResult : LoadMoreResult<LoadMoreSaleStaffResponse>;