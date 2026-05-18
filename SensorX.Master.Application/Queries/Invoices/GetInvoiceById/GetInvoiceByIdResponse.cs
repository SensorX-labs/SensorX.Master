namespace SensorX.Master.Application.Queries.Invoices.GetInvoiceById;

public record GetInvoiceByIdResponse(
    Guid Id,
    string Code,
    Guid OrderId,
    string CompanyName,
    string TaxCode,
    string Address,
    string Email,
    string InvoiceSymbol,
    string InvoiceNumber,
    string TaxAuthorityCode,
    string Status,
    DateTimeOffset IssueAt,
    decimal SubTotal,
    decimal TaxAmount,
    decimal GrandTotal,
    decimal AmountPaid,
    string ExpectedTransferSyntax,
    List<InvoiceItemResponse> Items
);

public record InvoiceItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string Unit,
    int Quantity,
    decimal UnitPrice,
    decimal TaxRate,
    decimal LineAmount,
    decimal TaxAmount,
    decimal TotalLineAmount
);
