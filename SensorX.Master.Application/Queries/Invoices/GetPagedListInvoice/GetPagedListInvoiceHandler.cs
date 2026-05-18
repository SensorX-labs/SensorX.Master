using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;

namespace SensorX.Master.Application.Queries.Invoices.GetPagedListInvoice;

public class GetPagedListInvoiceHandler(
    IQueryBuilder<Invoice> invoiceQueryBuilder,
    IQueryExecutor queryExecutor
) : IRequestHandler<GetPagedListInvoiceQuery, Result<OffsetPagedResult<GetPagedListInvoiceResponse>>>
{
    public async Task<Result<OffsetPagedResult<GetPagedListInvoiceResponse>>> Handle(
        GetPagedListInvoiceQuery request,
        CancellationToken cancellationToken)
    {
        var sourceQuery = invoiceQueryBuilder.QueryAsNoTracking;

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.Trim().ToLower();
            sourceQuery = sourceQuery.Where(x =>
                x.Code.Value.ToLower().Contains(searchTerm) ||
                x.BillingInfo.CompanyName.ToLower().Contains(searchTerm) ||
                x.BillingInfo.TaxCode.ToLower().Contains(searchTerm));
        }

        var totalCount = await queryExecutor.CountAsync(sourceQuery, cancellationToken);

        var pagedQuery = sourceQuery
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.IssueAt)
            .ApplyOffsetPagination(request);

        var invoices = await queryExecutor.ToListAsync(pagedQuery, cancellationToken);

        var items = invoices.Select(x => new GetPagedListInvoiceResponse(
            x.Id.Value,
            x.Code.Value,
            x.OrderId.Value,
            x.BillingInfo.CompanyName,
            x.BillingInfo.TaxCode,
            x.Status.ToString(),
            x.IssueAt,
            x.GrandTotal.Amount,
            x.AmountPaid.Amount,
            x.Items.Count,
            x.CreatedAt
        )).ToList();

        return Result<OffsetPagedResult<GetPagedListInvoiceResponse>>.Success(new OffsetPagedResult<GetPagedListInvoiceResponse>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        });
    }
}
