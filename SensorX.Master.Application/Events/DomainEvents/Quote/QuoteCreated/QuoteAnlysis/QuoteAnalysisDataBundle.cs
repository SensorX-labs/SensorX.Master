using MassTransit;

namespace SensorX.Master.Application.Events.DomainEvents.Quote.QuoteCreated.QuoteAnlysis;

[EntityName("quote-analysis-bundle")]
public sealed record QuoteAnalysisDataBundle(
    string QuoteId,
    string QuoteCode,
    CustomerAnalysisData Customer,
    StaffAnalysisData Staff,
    QuoteOverviewData Quote,
    DateTimeOffset GeneratedAt
);

public sealed record QuoteOverviewData(
    decimal TotalAmount,
    int ItemCount,
    decimal TotalQuantity,
    string? Note,
    List<AnalyzedItemData> Items
);

public sealed record AnalyzedItemData(
    string ProductCode,
    string ProductName,
    string Manufacturer,
    string Unit,
    decimal Quantity,
    decimal QuotedUnitPrice,
    decimal SuggestedPrice,
    decimal FloorPrice,
    List<PriceTierData> PriceTiers
);

public sealed record PriceTierData(decimal MinQuantity, decimal Price);

public sealed record CustomerAnalysisData(
    string CustomerId,
    string CompanyName,
    string RecipientName,
    int TotalQuotes,
    int AcceptedQuotes,
    int RejectedOrExpiredQuotes
);

public sealed record StaffAnalysisData(
    string StaffId,
    string StaffName,
    string Department,
    int TenureYears,
    int TenureMonths
);
