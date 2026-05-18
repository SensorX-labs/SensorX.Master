namespace SensorX.Master.Application.Queries.Orders.GetPageListOrder;

public record GetPageListOrderResponse(
    Guid Id,
    Guid QuoteId,
    string Code,
    Guid CustomerId,
    string RecipientName,
    string CompanyName,
    string Status,
    DateTimeOffset OrderDate,
    decimal GrandTotal,
    int ItemCount,
    DateTimeOffset CreatedAt
);
