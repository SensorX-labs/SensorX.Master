using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;

namespace SensorX.Master.Application.Queries.Quotes.GetDetailQuoteById;

public sealed record GetDetailQuoteByIdQuery(Guid QuoteId) : IRequest<Result<GetDetailQuoteByIdResponse>>;

public sealed record GetDetailQuoteByIdResponse
(
    Guid Id,
    string Code,
    Guid RFQId,
    string RFQCode,
    QuoteStatus Status,
    DateTimeOffset? QuoteDate,
    string? Note,
    string? ReasonReject,

    decimal Subtotal,
    decimal TotalTax,
    decimal GrandTotal,
    List<QuoteItemResponse> Items,

    SenderInfoResponse Sender,
    CustomerInfoResponse Customer,
    QuoteCustomerResponse? CustomerFeedback
);

public sealed record SenderInfoResponse
(
    Guid Id,
    string Name,
    string Email,
    string? Phone
);

public sealed record CustomerInfoResponse
(
    Guid Id,
    string CompanyName,
    string Phone,
    string Email,
    string Address,
    string TaxCode
);

public sealed record QuoteCustomerResponse
(
    QuoteResponseStatus? ResponseType,
    PaymentTerm? PaymentTerm,
    string? ShippingAddress,
    string? RecipientName,
    string? RecipientPhone,
    string? Feedback
);

public sealed record QuoteItemResponse
(
    Guid Id,
    Guid ProductId,
    string ProductCode,
    string ProductName,
    string Manufacturer,
    string Unit,
    int Quantity,
    decimal UnitPrice,
    decimal TaxRate,

    decimal LineAmount,
    decimal TaxAmount,
    decimal TotalLineAmount
);
