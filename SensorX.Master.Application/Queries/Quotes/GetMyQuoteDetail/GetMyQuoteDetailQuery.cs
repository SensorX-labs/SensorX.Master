using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;

namespace SensorX.Master.Application.Queries.Quotes.GetMyQuoteDetail;

public sealed record GetMyQuoteDetailQuery(Guid QuoteId) : IRequest<Result<GetMyQuoteDetailResponse>>;

public sealed record GetMyQuoteDetailResponse
(
    Guid Id,
    string Code,
    Guid RfqId,
    Guid? OrderId,
    string? OrderCode,
    StatusCustomerCanSeeQuote Status,
    DateTimeOffset? QuoteDate,
    decimal Subtotal,
    decimal TotalTax,
    decimal GrandTotal,
    bool IsStockSufficient,
    List<QuoteItemResponse> Items,

    SenderInfoResponse Sender,
    CustomerInfoResponse Customer
);

public enum StatusCustomerCanSeeQuote
{
    Pending,
    Accepted,
    Declined,
    Expired
}

public sealed record SenderInfoResponse
(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    string? AvatarUrl
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

public sealed record QuoteItemResponse
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
