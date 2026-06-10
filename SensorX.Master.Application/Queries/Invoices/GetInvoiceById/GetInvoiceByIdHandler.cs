using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Application.Queries.Invoices.GetInvoiceById;

public class GetInvoiceByIdHandler(
    IQueryBuilder<Invoice> invoiceQueryBuilder,
    IQueryExecutor queryExecutor
) : IRequestHandler<GetInvoiceByIdQuery, Result<GetInvoiceByIdResponse>>
{
    public async Task<Result<GetInvoiceByIdResponse>> Handle(
        GetInvoiceByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var invoice = await queryExecutor.FirstOrDefaultAsync(
                invoiceQueryBuilder.QueryAsNoTracking.Where(x => x.Id == new InvoiceId(request.InvoiceId)),
                cancellationToken);

            if (invoice is null)
            {
                return Result<GetInvoiceByIdResponse>.Failure("Khong tim thay hoa don");
            }

            var response = new GetInvoiceByIdResponse(
                invoice.Id.Value,
                invoice.Code.Value,
                invoice.OrderId.Value,
                invoice.BillingInfo.CompanyName,
                invoice.BillingInfo.TaxCode,
                invoice.BillingInfo.Address,
                invoice.BillingInfo.Email.Value,
                invoice.InvoiceSymbol,
                invoice.InvoiceNumber,
                invoice.TaxAuthorityCode,
                invoice.Status.ToString(),
                invoice.IssueAt,
                invoice.SubTotal.Amount,
                invoice.TaxAmount.Amount,
                invoice.GrandTotal.Amount,
                invoice.AmountPaid.Amount,
                invoice.GetExpectedTransferSyntax(),
                invoice.Items.Select(i => new InvoiceItemResponse(
                    i.Id.Value,
                    i.ProductId.Value,
                    i.ProductName,
                    i.Unit,
                    i.Quantity.Value,
                    i.UnitPrice.Amount,
                    i.TaxRate.Value,
                    i.LineAmount.Amount,
                    i.TaxAmount.Amount,
                    i.TotalLineAmount.Amount
                )).ToList()
            );

            return Result<GetInvoiceByIdResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<GetInvoiceByIdResponse>.Failure($"Loi khi lay chi tiet hoa don: {ex.Message}");
        }
    }
}
