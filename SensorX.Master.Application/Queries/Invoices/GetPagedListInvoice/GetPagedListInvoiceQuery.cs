using MediatR;
using SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.Invoices.GetPagedListInvoice;

public record GetPagedListInvoiceQuery : OffsetPagedQuery, IRequest<Result<OffsetPagedResult<GetPagedListInvoiceResponse>>>
{
    public string? SearchTerm { get; init; }
    public string? Status { get; init; }
    public string? Code { get; init; }
    public string? OrderCode { get; init; }
    public string? CompanyName { get; init; }
    public string? TaxCode { get; init; }
    public string? Email { get; init; }
    public string? Address { get; init; }
    public decimal? TotalFrom { get; init; }
    public decimal? TotalTo { get; init; }
    public decimal? AmountPaidFrom { get; init; }
    public decimal? AmountPaidTo { get; init; }
    public DateTimeOffset? IssueFrom { get; init; }
    public DateTimeOffset? IssueTo { get; init; }
    public DateTimeOffset? CreatedFrom { get; init; }
    public DateTimeOffset? CreatedTo { get; init; }
}
