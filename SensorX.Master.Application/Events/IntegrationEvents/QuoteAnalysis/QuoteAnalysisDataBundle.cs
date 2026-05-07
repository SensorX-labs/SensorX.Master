using MassTransit;

namespace SensorX.Master.Application.Events.IntegrationEvents.QuoteAnalysis;

[EntityName("quote-analysis-bundle")]
public record QuoteAnalysisDataBundle(
    string QuoteId,
    string QuoteCode,
    CustomerAnalysisData Customer,
    StaffAnalysisData Staff,
    QuoteOverviewData Quote,
    DateTimeOffset GeneratedAt
);

public record QuoteOverviewData(
    decimal TotalAmount,
    int ItemCount,
    decimal TotalQuantity,
    string? Note,
    List<AnalyzedItemData> Items
);

public record AnalyzedItemData(
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

public record PriceTierData(int MinQuantity, decimal Price);

public record CustomerAnalysisData(
    string CustomerId,
    string CompanyName,
    string RecipientName,
    int TotalQuotes,            // Tổng số báo giá
    int AcceptedQuotes,         // Số báo giá đã chốt (Ordered)
    int RejectedOrExpiredQuotes // Số báo giá thất bại (Rejected/Expired)
);

public record StaffAnalysisData(
    string StaffId,
    string StaffName,
    string Department,
    int TenureYears // Số năm kinh nghiệm
);
