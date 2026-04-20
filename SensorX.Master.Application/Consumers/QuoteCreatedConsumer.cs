using MassTransit;
using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.IntegrationEvents;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using Microsoft.EntityFrameworkCore;
using SensorX.Master.Application.Common.Models.DataServiceModels;

namespace SensorX.Master.Application.Consumers;

public class QuoteCreatedConsumer : IConsumer<QuoteCreatedIntegrationEvent>
{
    private readonly IDataServiceClient _dataClient;
    private readonly IRepository<Quote> _quoteRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<RFQ> _rfqRepository;
    private readonly IRepository<Invoice> _invoiceRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<QuoteCreatedConsumer> _logger;

    public QuoteCreatedConsumer(
        IDataServiceClient dataClient,
        IRepository<Quote> quoteRepository,
        IRepository<Order> orderRepository,
        IRepository<RFQ> rfqRepository,
        IRepository<Invoice> invoiceRepository,
        IPublishEndpoint publishEndpoint,
        ILogger<QuoteCreatedConsumer> logger)
    {
        _dataClient = dataClient;
        _quoteRepository = quoteRepository;
        _orderRepository = orderRepository;
        _rfqRepository = rfqRepository;
        _invoiceRepository = invoiceRepository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<QuoteCreatedIntegrationEvent> context)
    {
        var eventData = context.Message;
        _logger.LogInformation(">>> [AI-Enrichment] Bắt đầu tổng hợp dữ liệu báo giá có tính liên kết cao: {QuoteId}", eventData.QuoteId);
        
        try
        {
            var quote = await _quoteRepository.GetByIdAsync(new QuoteId(eventData.QuoteId));
            if (quote == null) return;

            // 1. Thu thập dữ liệu
            var productIds = quote.LineItems.Select(x => x.ProductId.Value).ToArray();
            var customerHistoryTask = _dataClient.GetCustomerHistoryAsync(quote.CustomerId.Value);
            var pricingPolicyTask = _dataClient.GetProductPricingAsync(productIds);
            var staffMetricsTask = _dataClient.GetEmployeeMetricsAsync(Guid.Empty); // TODO: Salesperson ID

            var orders = await _orderRepository.AsQueryable().Where(x => x.CustomerId == quote.CustomerId).ToListAsync();
            var rfqs = await _rfqRepository.AsQueryable().Where(x => x.CustomerId == quote.CustomerId).ToListAsync();
            var invoices = await (from inv in _invoiceRepository.AsQueryable()
                                 join ord in _orderRepository.AsQueryable() on inv.OrderId equals ord.Id
                                 where ord.CustomerId == quote.CustomerId
                                 select inv).ToListAsync();

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
                        Tiers: policy?.PriceTiers?.Select(t => new SensorX.Master.Application.IntegrationEvents.PriceTierData(t.Quantity, t.Price)).ToList() ?? []
                    ),
                    Margin: margin
                ));
            }

            // 3. Phân tích Khách hàng & Sales
            var totalOrders = orders.Count;
            var lastOrderDays = orders.OrderByDescending(o => o.OrderDate).FirstOrDefault() is Order o 
                ? (DateTimeOffset.UtcNow - o.OrderDate).Days : 365;
            var overdueInvoices = invoices.Count(i => (i.Status == InvoiceStatus.Unpaid || i.Status == InvoiceStatus.PartiallyPaid) 
                                                      && (DateTimeOffset.UtcNow - i.IssueAt).Days > 30);
            var staffTenureYears = extStaff.Data != null ? (DateTime.UtcNow.Year - extStaff.Data.CreatedAt.Year) : 0;

            // 4. Đóng gói Bundle Final
            var bundle = new QuoteAnalysisDataBundle
            {
                QuoteId = quote.Code.Value,
                Customer = new CustomerAnalysisData(
                    IsExisting: totalOrders > 0 || (extCustomer.Data != null && (DateTime.UtcNow - extCustomer.Data.CreatedDate).Days > 60),
                    TotalOrders: totalOrders,
                    LastOrderDaysAgo: lastOrderDays,
                    AvgOrderValue: totalOrders > 0 ? orders.Average(o => o.GetGrandTotal().Amount) : 0,
                    PaymentBehavior: overdueInvoices > 0 ? "often_late" : "on_time",
                    RelationshipLevel: totalOrders > 5 ? "high" : "medium",
                    RfqsWithoutOrders: rfqs.Count(r => r.Status == RFQStatus.Rejected)
                ),
                Quote = new QuoteOverviewData(
                    TotalAmount: quote.GetGrandTotal().Amount,
                    TotalSuggestedPrice: extPricing.Sum(p => p.SuggestedPrice),
                    TotalFloorPrice: extPricing.Sum(p => p.FloorPrice),
                    AvgMargin: analyzedItems.Count > 0 ? analyzedItems.Average(i => i.Margin) : 0,
                    ItemCount: quote.LineItems.Count,
                    TotalItemCount: quote.LineItems.Sum(i => i.Quantity.Value),
                    Items: analyzedItems, // Danh sách items chứa đầy đủ thông tin bên trong
                    Complexity: quote.LineItems.Count > 5 ? "high" : "low"
                ),
                Context = new ContextData(
                    Urgency: "medium",
                    Competition: true,
                    CustomerRequestedQuote: quote.RFQId != null,
                    DeadlineDays: 7
                ),
                Sales = new SalesAnalysisData(
                    ExperienceYears: staffTenureYears,
                    WinRate: 0.75,
                    RecentPerformance: staffTenureYears > 2 ? "senior" : "junior"
                ),
                CustomerMessage = quote.Note ?? "Không có ghi chú.",
            };

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
