namespace SensorX.Master.Application.Queries.Quotes.GetDetailQuoteById;

public record GetDetailQuoteByIdResponse
(
    Guid Id,
    string Code,
    Guid RFQId,
    Guid CustomerId,
    string Status,
    DateTimeOffset QuoteDate,
    string? Note,
    string? ReasonReject,

    // thong tin khách hàng
    string RecipientName,
    string RecipientPhone,
    string CompanyName,
    string Email,
    string Address,
    string TaxCode,

    // feedback
    string? CustomerResponseType,
    string? ShippingAddress,
    string? PaymentTerm,
    string? CustomerFeedback,

    decimal Subtotal,
    decimal TotalTax,
    decimal GrandTotal,

    List<QuoteItemResponse> Items
);

public record QuoteItemResponse
(
    Guid Id,
    Guid ProductId,
    string ProductCode,
    string Manufacturer,
    string Unit,
    int Quantity,
    decimal UnitPrice,
    decimal TaxRate,

    decimal LineAmount,
    decimal TaxAmount,
    decimal TotalLineAmount
);
