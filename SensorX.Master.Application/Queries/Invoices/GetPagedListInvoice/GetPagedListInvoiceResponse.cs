namespace SensorX.Master.Application.Queries.Invoices.GetPagedListInvoice;

public record GetPagedListInvoiceResponse(
    Guid Id,
    string Code,
    Guid OrderId,
    string CompanyName,
    string TaxCode,
    string Status,
    DateTimeOffset IssueAt,
    decimal GrandTotal,
    decimal AmountPaid,
    int ItemCount,
    DateTimeOffset CreatedAt
);
