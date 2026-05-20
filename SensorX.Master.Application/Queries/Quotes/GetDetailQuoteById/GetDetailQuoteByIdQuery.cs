using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;

namespace SensorX.Master.Application.Queries.Quotes.GetDetailQuoteById;

public record GetDetailQuoteByIdQuery(Guid QuoteId) : IRequest<Result<GetDetailQuoteByIdResponse>>;

public record GetDetailQuoteByIdResponse
(
    Guid Id,
    string Code,
    Guid RFQId,
    Guid CustomerId,
    QuoteStatus Status,
    DateTimeOffset? QuoteDate,
    string? Note,
    string? ReasonReject,

    // thong tin khách hàng
    string CompanyName,
    string Phone,
    string Email,
    string Address,
    string TaxCode,

    // feedback customer
    QuoteResponseStatus? ResponseType,
    PaymentTerm? PaymentTerm,
    string? ShippingAddress,
    string? RecipientName,
    string? RecipientPhone,
    string? Feedback,

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