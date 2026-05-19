namespace SensorX.Master.Application.Queries.Quotes.GetQuoteByCode;

public record GetQuoteByCodeResponse(
    Guid Id,
    string Code,
    string Status,
    DateTimeOffset QuoteDate,
    Guid CustomerId,
    string RecipientName,
    string CompanyName,
    decimal GrandTotal,
    int ItemCount,
    DateTimeOffset CreatedAt
);
