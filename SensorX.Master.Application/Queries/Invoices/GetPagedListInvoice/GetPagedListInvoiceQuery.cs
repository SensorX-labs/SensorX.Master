using MediatR;
using SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.Invoices.GetPagedListInvoice;

public record GetPagedListInvoiceQuery : OffsetPagedQuery, IRequest<Result<OffsetPagedResult<GetPagedListInvoiceResponse>>>
{
    public string? SearchTerm { get; init; }
}
