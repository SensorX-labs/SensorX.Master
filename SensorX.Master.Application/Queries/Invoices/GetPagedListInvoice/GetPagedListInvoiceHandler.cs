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
            if (Guid.TryParse(request.SearchTerm.Trim(), out var orderId))
            {
                sourceQuery = sourceQuery.Where(x =>
                    ((string)x.Code).ToLower().Contains(searchTerm) ||
                    x.OrderId.Value == orderId ||
                    x.BillingInfo.CompanyName.ToLower().Contains(searchTerm) ||
                    x.BillingInfo.TaxCode.ToLower().Contains(searchTerm));
            }
            else
            {
                sourceQuery = sourceQuery.Where(x =>
                    ((string)x.Code).ToLower().Contains(searchTerm) ||
                    x.BillingInfo.CompanyName.ToLower().Contains(searchTerm) ||
                    x.BillingInfo.TaxCode.ToLower().Contains(searchTerm));
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<InvoiceStatus>(request.Status, true, out var status))
        {
            sourceQuery = sourceQuery.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var code = request.Code.Trim().ToLower();
            sourceQuery = sourceQuery.Where(x => ((string)x.Code).ToLower().Contains(code));
        }

        if (!string.IsNullOrWhiteSpace(request.OrderCode))
        {
            if (Guid.TryParse(request.OrderCode.Trim(), out var orderId))
            {
                sourceQuery = sourceQuery.Where(x => x.OrderId.Value == orderId);
            }
        }

        if (!string.IsNullOrWhiteSpace(request.CompanyName))
        {
            var companyName = request.CompanyName.Trim().ToLower();
            sourceQuery = sourceQuery.Where(x => x.BillingInfo.CompanyName.ToLower().Contains(companyName));
        }

        if (!string.IsNullOrWhiteSpace(request.TaxCode))
        {
            var taxCode = request.TaxCode.Trim().ToLower();
            sourceQuery = sourceQuery.Where(x => x.BillingInfo.TaxCode.ToLower().Contains(taxCode));
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var email = request.Email.Trim().ToLower();
            sourceQuery = sourceQuery.Where(x => ((string)x.BillingInfo.Email).ToLower().Contains(email));
        }

        if (!string.IsNullOrWhiteSpace(request.Address))
        {
            var address = request.Address.Trim().ToLower();
            sourceQuery = sourceQuery.Where(x => x.BillingInfo.Address.ToLower().Contains(address));
        }

        if (request.IssueFrom.HasValue)
        {
            sourceQuery = sourceQuery.Where(x => x.IssueAt >= request.IssueFrom.Value);
        }

        if (request.IssueTo.HasValue)
        {
            sourceQuery = sourceQuery.Where(x => x.IssueAt <= request.IssueTo.Value);
        }

        if (request.CreatedFrom.HasValue)
        {
            sourceQuery = sourceQuery.Where(x => x.CreatedAt >= request.CreatedFrom.Value);
        }

        if (request.CreatedTo.HasValue)
        {
            sourceQuery = sourceQuery.Where(x => x.CreatedAt <= request.CreatedTo.Value);
        }

        var hasAmountFilter =
            request.TotalFrom.HasValue ||
            request.TotalTo.HasValue ||
            request.AmountPaidFrom.HasValue ||
            request.AmountPaidTo.HasValue;

        if (hasAmountFilter)
        {
            var orderedInvoices = sourceQuery
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.IssueAt);

            var materializedInvoices = await queryExecutor.ToListAsync(orderedInvoices, cancellationToken);

            var filteredInvoices = materializedInvoices.Select(x => new
            {
                Invoice = x,
                GrandTotal = x.GrandTotal.Amount,
                AmountPaid = x.AmountPaid.Amount
            });

            if (request.TotalFrom.HasValue)
            {
                filteredInvoices = filteredInvoices.Where(x => x.GrandTotal >= request.TotalFrom.Value);
            }

            if (request.TotalTo.HasValue)
            {
                filteredInvoices = filteredInvoices.Where(x => x.GrandTotal <= request.TotalTo.Value);
            }

            if (request.AmountPaidFrom.HasValue)
            {
                filteredInvoices = filteredInvoices.Where(x => x.AmountPaid >= request.AmountPaidFrom.Value);
            }

            if (request.AmountPaidTo.HasValue)
            {
                filteredInvoices = filteredInvoices.Where(x => x.AmountPaid <= request.AmountPaidTo.Value);
            }

            var filteredInvoiceList = filteredInvoices.ToList();
            var totalCountWithAmount = filteredInvoiceList.Count;
            var pageNumber = request.PageNumber ?? 1;
            var pageSize = request.PageSize ?? 10;

            var pagedItems = filteredInvoiceList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new GetPagedListInvoiceResponse(
                    x.Invoice.Id.Value,
                    x.Invoice.Code.Value,
                    x.Invoice.OrderId.Value,
                    x.Invoice.BillingInfo.CompanyName,
                    x.Invoice.BillingInfo.TaxCode,
                    x.Invoice.Status.ToString(),
                    x.Invoice.IssueAt,
                    x.GrandTotal,
                    x.AmountPaid,
                    x.Invoice.Items.Count,
                    x.Invoice.CreatedAt
                )).ToList();

            return Result<OffsetPagedResult<GetPagedListInvoiceResponse>>.Success(new OffsetPagedResult<GetPagedListInvoiceResponse>
            {
                Items = pagedItems,
                TotalCount = totalCountWithAmount,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
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
            PageNumber = request.PageNumber ?? 1,
            PageSize = request.PageSize ?? 10
        });
    }
}
