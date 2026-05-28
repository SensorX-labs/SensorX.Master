using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.Invoices.GetInvoiceStats;

public record GetInvoiceStatsQuery : IRequest<Result<GetInvoiceStatsResponse>>;

public class GetInvoiceStatsResponse
{
    public int TotalCount { get; init; }
    public int UnpaidCount { get; init; }
    public int PartiallyPaidCount { get; init; }
    public int PaidCount { get; init; }
    public int IssuedCount { get; init; }
    public int CancelledCount { get; init; }
}
