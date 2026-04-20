using MassTransit;
using SensorX.Master.Application.Common.Models.DataServiceModels;

namespace SensorX.Master.Application.Events.IntegrationEvents.QuoteAnalysis;

[MessageUrn("quote-analysis-data-bundle")]
public record QuoteAnalysisDataBundle(
    string QuoteId,
    CustomerAnalysisData Customer,
    QuoteOverviewData Quote, // Chứa toàn bộ thông tin về báo giá và sản phẩm
    ContextData Context,
    SalesAnalysisData Sales,
    string CustomerMessage
)
{
    public DateTimeOffset GeneratedAt { get; init; } = DateTimeOffset.UtcNow;
};

public record QuoteOverviewData(
    decimal TotalAmount,
    decimal TotalSuggestedPrice,
    decimal TotalFloorPrice,
    decimal AvgMargin,
    int ItemCount,
    decimal TotalItemCount,
    List<AnalyzedItemData> Items, // Danh sách sản phẩm nằm ngay trong Quote
    string Complexity
);

public record AnalyzedItemData(
    string ProductCode,
    string ProductName,
    decimal Quantity,
    decimal QuotedPrice, // Giá mà nhân viên đang báo cho khách
    ItemPricingPolicyData Policy, // Chính sách giá nội bộ đi kèm
    decimal Margin // Biên lợi nhuận (%)
);

public record ItemPricingPolicyData(
    decimal SuggestedPrice,
    decimal FloorPrice,
    List<PriceTierData> Tiers
);

public record CustomerAnalysisData(
    bool IsExisting,
    int TotalOrders,
    int LastOrderDaysAgo,
    decimal AvgOrderValue,
    string PaymentBehavior,
    string RelationshipLevel,
    int RfqsWithoutOrders
);

public record PriceTierData(int Quantity, decimal Price);

public record ContextData(
    string Urgency,
    bool Competition,
    bool CustomerRequestedQuote,
    int DeadlineDays
);

public record SalesAnalysisData(
    int ExperienceYears,
    double WinRate,
    string RecentPerformance
);
