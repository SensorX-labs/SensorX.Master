namespace SensorX.Master.Application.Queries.Orders.GetDetailOrderById;

public record GetDetailOrderByIdResponse(
    Guid Id,
    Guid QuoteId,
    string Code,
    Guid CustomerId,
    string Status,
    DateTimeOffset OrderDate,

    string RecipientName,
    string RecipientPhone,
    string CompanyName,
    string Email,
    string Address,
    string TaxCode,

    string SenderName,
    string SenderEmail,

    decimal Subtotal,
    decimal TotalTax,
    decimal GrandTotal,

    Guid? PaymentId,
    string? PaymentStatus,
    string? PaymentType,
    List<string>? PaymentQRURls,
    decimal? PaymentAmount,

    List<OrderItemResponse> Items
);

public record OrderItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductCode,
    string ProductName,
    string Manufacturer,
    string Unit,
    int Quantity,
    decimal UnitPrice,
    decimal TaxRate,
    string? Note,
    decimal LineAmount,
    decimal TaxAmount,
    decimal TotalLineAmount
);
