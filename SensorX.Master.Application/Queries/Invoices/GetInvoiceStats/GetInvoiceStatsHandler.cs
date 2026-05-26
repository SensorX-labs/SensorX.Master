using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;

namespace SensorX.Master.Application.Queries.Invoices.GetInvoiceStats;

public class GetInvoiceStatsHandler(
    IQueryBuilder<Invoice> invoiceQueryBuilder,
    IQueryExecutor queryExecutor
) : IRequestHandler<GetInvoiceStatsQuery, Result<GetInvoiceStatsResponse>>
{
    public async Task<Result<GetInvoiceStatsResponse>> Handle(GetInvoiceStatsQuery request, CancellationToken cancellationToken)
    {
        var query = invoiceQueryBuilder.QueryAsNoTracking;

        var totalCount = await queryExecutor.CountAsync(query, cancellationToken);
        var unpaidCount = await queryExecutor.CountAsync(query.Where(x => x.Status == InvoiceStatus.Unpaid), cancellationToken);
        var partiallyPaidCount = await queryExecutor.CountAsync(query.Where(x => x.Status == InvoiceStatus.PartiallyPaid), cancellationToken);
        var paidCount = await queryExecutor.CountAsync(query.Where(x => x.Status == InvoiceStatus.Paid), cancellationToken);
        var issuedCount = await queryExecutor.CountAsync(query.Where(x => x.Status == InvoiceStatus.Issued), cancellationToken);
        var cancelledCount = await queryExecutor.CountAsync(query.Where(x => x.Status == InvoiceStatus.Cancelled), cancellationToken);

        return Result<GetInvoiceStatsResponse>.Success(new GetInvoiceStatsResponse
        {
            TotalCount = totalCount,
            UnpaidCount = unpaidCount,
            PartiallyPaidCount = partiallyPaidCount,
            PaidCount = paidCount,
            IssuedCount = issuedCount,
            CancelledCount = cancelledCount
        });
    }
}
