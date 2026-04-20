using MassTransit;
using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.Models.DataServiceModels;
using SensorX.Master.Application.Events.DomainEvents.QuoteCreated;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Application.Events.IntegrationEvents.QuoteAnalysis;

public class QuoteAnalysisIntegrationEvent(
    IDataServiceClient _dataClient,
    IQueryBuilder<Quote> _quoteRepository,
    IQueryBuilder<Order> _orderRepository,
    IQueryBuilder<RFQ> _rfqRepository,
    IQueryBuilder<Invoice> _invoiceRepository,
    IQueryExecutor _queryExecutor,
    IPublishEndpoint _publishEndpoint,
    ILogger<QuoteAnalysisIntegrationEvent> _logger
) : IConsumer<IQuoteCreatedEvent>
{
    public async Task Consume(ConsumeContext<IQuoteCreatedEvent> context)
    {
        var eventData = context.Message;
        _logger.LogInformation(">>> [AI-Enrichment] Bắt đầu tổng hợp dữ liệu báo giá có tính liên kết cao: {QuoteId}", eventData.QuoteId);

        try
        {
            var quoteQuery = _quoteRepository.QueryAsNoTracking.Where(x => x.Id == new QuoteId(eventData.QuoteId));
            var quote = await _queryExecutor.FirstOrDefaultAsync(quoteQuery);
            if (quote is null) return;

            // 1. Thu thập dữ liệu
            var productIds = quote.LineItems.Select(x => x.ProductId.Value).ToArray();
            var customerHistoryTask = _dataClient.GetCustomerHistoryAsync(quote.CustomerId.Value);
            var pricingPolicyTask = _dataClient.GetProductPricingAsync(productIds);
            var staffMetricsTask = _dataClient.GetEmployeeMetricsAsync(Guid.Empty); // TODO: Salesperson ID

            var ordersQuery = _orderRepository.QueryAsNoTracking.Where(x => x.CustomerId == quote.CustomerId);
            var orders = await _queryExecutor.ToListAsync(ordersQuery);

            var rfqsQuery = _rfqRepository.QueryAsNoTracking.Where(x => x.CustomerId == quote.CustomerId);
            var rfqs = await _queryExecutor.ToListAsync(rfqsQuery);

            var invoicesQuery = from inv in _invoiceRepository.QueryAsNoTracking
                                join ord in _orderRepository.QueryAsNoTracking on inv.OrderId equals ord.Id
                                where ord.CustomerId == quote.CustomerId
                                select inv;
            var invoices = await _queryExecutor.ToListAsync(invoicesQuery);

            await Task.WhenAll(customerHistoryTask, pricingPolicyTask, staffMetricsTask);

            var extCustomer = await customerHistoryTask;
            var extPricing = await pricingPolicyTask;
            var extStaff = await staffMetricsTask;

            // 2. Phân tích chi tiết từng sản phẩm
            var analyzedItems = new List<AnalyzedItemData>();
            foreach (var item in quote.LineItems)
            {
                var policy = extPricing.FirstOrDefault(p => p.ProductId == item.ProductId.Value);
                decimal floorPrice = policy?.FloorPrice ?? 0;
                decimal suggestedPrice = policy?.SuggestedPrice ?? 0;

                decimal margin = item.UnitPrice.Amount > 0
                    ? Math.Round(((item.UnitPrice.Amount - floorPrice) / item.UnitPrice.Amount) * 100, 2)
                    : 0;

                analyzedItems.Add(new AnalyzedItemData(
                    ProductCode: item.ProductCode.Value,
                    ProductName: policy?.ProductName ?? item.ProductCode.Value,
                    Quantity: item.Quantity.Value,
                    QuotedPrice: item.UnitPrice.Amount,
                    Policy: new ItemPricingPolicyData(
                        SuggestedPrice: suggestedPrice,
                        FloorPrice: floorPrice,
                        Tiers: policy?.PriceTiers?.Select(t => new PriceTierData(t.Quantity, t.Price)).ToList() ?? []
                    ),
                    Margin: margin
                ));
            }

            // 3. Phân tích Khách hàng & Sales
            var totalOrders = orders.Count;
            var lastOrderDays = orders.OrderByDescending(o => o.OrderDate)
                                      .FirstOrDefault() is Order o ? (DateTimeOffset.UtcNow - o.OrderDate).Days : 365;
            var overdueInvoices = invoices.Count(i => (i.Status == InvoiceStatus.Unpaid
                                                        || i.Status == InvoiceStatus.PartiallyPaid
                                                    ) && (DateTimeOffset.UtcNow - i.IssueAt).Days > 30);
            var staffTenureYears = extStaff.Data != null ? (DateTime.UtcNow.Year - extStaff.Data.CreatedAt.Year) : 0;

            // 4. Đóng gói Bundle Final
            var bundle = new QuoteAnalysisDataBundle
            (
                QuoteId: quote.Code.Value,
                Customer: new CustomerAnalysisData(
                    IsExisting: totalOrders > 0 || (extCustomer.Data != null && (DateTime.UtcNow - extCustomer.Data.CreatedDate).Days > 60),
                    TotalOrders: totalOrders,
                    LastOrderDaysAgo: lastOrderDays,
                    AvgOrderValue: totalOrders > 0 ? orders.Average(o => o.GetGrandTotal().Amount) : 0,
                    PaymentBehavior: overdueInvoices > 0 ? "often_late" : "on_time",
                    RelationshipLevel: totalOrders > 5 ? "high" : "medium",
                    RfqsWithoutOrders: rfqs.Count(r => r.Status == RFQStatus.Rejected)
                ),
                Quote: new QuoteOverviewData(
                    TotalAmount: quote.GetGrandTotal().Amount,
                    TotalSuggestedPrice: extPricing.Sum(p => p.SuggestedPrice),
                    TotalFloorPrice: extPricing.Sum(p => p.FloorPrice),
                    AvgMargin: analyzedItems.Count > 0 ? analyzedItems.Average(i => i.Margin) : 0,
                    ItemCount: quote.LineItems.Count,
                    TotalItemCount: quote.LineItems.Sum(i => i.Quantity.Value),
                    Items: analyzedItems, // Danh sách items chứa đầy đủ thông tin bên trong
                    Complexity: quote.LineItems.Count > 5 ? "high" : "low"
                ),
                Context: new ContextData(
                    Urgency: "medium",
                    Competition: true,
                    CustomerRequestedQuote: quote.RFQId != null,
                    DeadlineDays: 7
                ),
                Sales: new SalesAnalysisData(
                    ExperienceYears: staffTenureYears,
                    WinRate: 0.75,
                    RecentPerformance: staffTenureYears > 2 ? "senior" : "junior"
                ),
                CustomerMessage: quote.Note ?? "Không có ghi chú."
            );

            await _publishEndpoint.Publish(bundle);
            _logger.LogInformation(">>> [AI-Enrichment] Hoàn tất Bundle có tính liên kết cao cho Báo giá {QuoteCode}.", quote.Code.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ">>> [AI-Enrichment] Lỗi khi tổng hợp dữ liệu Bundle.");
            throw;
        }
    }
}
