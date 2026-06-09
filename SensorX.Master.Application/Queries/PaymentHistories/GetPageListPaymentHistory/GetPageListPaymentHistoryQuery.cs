using MediatR;
using SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.PaymentHistories.GetPageListPaymentHistory;

public record GetPageListPaymentHistoryQuery : OffsetPagedQuery, IRequest<Result<OffsetPagedResult<GetPageListPaymentHistoryResponse>>>
{
    public string? SearchTerm { get; init; }
    public string? Gateway { get; init; }
    public Guid? PaymentId { get; init; }
    public Guid? OrderId { get; init; }
    public string? Status { get; init; }
}
