using MassTransit;
using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.Models.DataServiceModels;
using SensorX.Master.Application.Events.DomainEvents.QuoteCreated;
using SensorX.Master.Domain.Contexts.QuoteContext;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Application.Events.IntegrationEvents.QuoteAnalysis;

public class QuoteAnalysisIntegrationEvent : IConsumer<IQuoteCreatedEvent>
{
    private readonly IDataServiceClient _dataClient;
    private readonly IQueryBuilder<Quote> _quoteQueryBuilder;
    private readonly IQueryBuilder<RFQ> _rfqQueryBuilder;
    private readonly IQueryExecutor _queryExecutor;
    private readonly ILogger<QuoteAnalysisIntegrationEvent> _logger;

    public QuoteAnalysisIntegrationEvent(
        IDataServiceClient dataClient,
        IQueryBuilder<Quote> quoteQueryBuilder,
        IQueryBuilder<RFQ> rfqQueryBuilder,
        IQueryExecutor queryExecutor,
        ILogger<QuoteAnalysisIntegrationEvent> logger)
    {
        _dataClient = dataClient;
        _quoteQueryBuilder = quoteQueryBuilder;
        _rfqQueryBuilder = rfqQueryBuilder;
        _queryExecutor = queryExecutor;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IQuoteCreatedEvent> context)
    {
        var eventData = context.Message;
        _logger.LogInformation(">>> [AI-Enrichment] Bắt đầu lấy dữ liệu cho báo giá: {QuoteId}", eventData.QuoteId);

        try
        {
            // 1. Load báo giá hiện tại
            var quoteQuery = _quoteQueryBuilder.QueryAsNoTracking.Where(x => x.Id == new QuoteId(eventData.QuoteId));
            var quote = await _queryExecutor.FirstOrDefaultAsync(quoteQuery);
            if (quote is null) return;

            // 2. Tìm StaffId từ RFQ
            Guid? staffId = null;
            if (quote.RFQId != null)
            {
                var rfqQuery = _rfqQueryBuilder.QueryAsNoTracking.Where(x => x.Id == quote.RFQId);
                var rfq = await _queryExecutor.FirstOrDefaultAsync(rfqQuery);
                staffId = rfq?.StaffId?.Value;
            }

            // 3. Lấy dữ liệu sản phẩm và nhân viên từ Data Service
            var productIds = quote.LineItems.Select(x => x.ProductId.Value).ToArray();
            var pricingTask = _dataClient.GetProductPricingAsync(productIds);
            var staffTask = staffId.HasValue 
                ? _dataClient.GetEmployeeMetricsAsync(staffId.Value) 
                : Task.FromResult<StaffMetricsApiResponse>(new StaffMetricsApiResponse { IsSuccess = false });

            // 4. Đếm số lượng báo giá của khách hàng
            var allQuotesQuery = _quoteQueryBuilder.QueryAsNoTracking
                .Where(x => x.CustomerId == quote.CustomerId && x.Id != quote.Id);
            var customerQuotes = await _queryExecutor.ToListAsync(allQuotesQuery);

            await Task.WhenAll(pricingTask, staffTask);
            var extPricing = await pricingTask;
            var extStaff = await staffTask;

            // 5. Chuẩn bị danh sách items thô
            var analyzedItems = quote.LineItems.Select(item =>
            {
                var policy = extPricing.FirstOrDefault(p => p.ProductId == item.ProductId.Value);
                return new AnalyzedItemData(
                    ProductCode: item.ProductCode.Value,
                    ProductName: policy?.ProductName ?? item.ProductCode.Value,
                    Manufacturer: policy?.Manufacture ?? item.Manufacturer,
                    Unit: item.Unit,
                    Quantity: item.Quantity.Value,
                    QuotedUnitPrice: item.UnitPrice.Amount,
                    SuggestedPrice: policy?.SuggestedPrice ?? 0,
                    FloorPrice: policy?.FloorPrice ?? 0,
                    PriceTiers: policy?.PriceTiers?.Select(t => new PriceTierData(t.Quantity, t.Price)).ToList() ?? []
                );
            }).ToList();

            // 7. Thông tin nhân viên và Năm kinh nghiệm
            var staffData = extStaff?.Value;
            var tenureYears = staffData != null 
                ? Math.Max(0, DateTime.UtcNow.Year - staffData.CreatedAt.Year) 
                : 0;

            // 8. Đóng gói Bundle tối giản
            var bundle = new QuoteAnalysisDataBundle(
                QuoteId: quote.Id.Value.ToString(),
                QuoteCode: quote.Code.Value,
                Customer: new CustomerAnalysisData(
                    CustomerId: quote.CustomerId.Value.ToString(),
                    CompanyName: quote.CustomerInfo.CompanyName,
                    RecipientName: string.Empty,
                    TotalQuotes: customerQuotes.Count,
                    AcceptedQuotes: customerQuotes.Count(q => q.Status == QuoteStatus.Ordered),
                    RejectedOrExpiredQuotes: customerQuotes.Count(q => q.Status == QuoteStatus.Returned || q.Status == QuoteStatus.Expired)
                ),
                Staff: new StaffAnalysisData(
                    StaffId: staffId?.ToString() ?? "N/A",
                    StaffName: staffData?.StaffName ?? "Chưa gán",
                    Department: staffData?.Department ?? "N/A",
                    TenureYears: tenureYears
                ),
                Quote: new QuoteOverviewData(
                    TotalAmount: quote.GetGrandTotal().Amount,
                    ItemCount: analyzedItems.Count,
                    TotalQuantity: analyzedItems.Sum(i => i.Quantity),
                    Note: quote.Note,
                    Items: analyzedItems
                ),
                GeneratedAt: DateTimeOffset.UtcNow
            );

            await context.Publish(bundle);
            _logger.LogInformation(">>> [AI-Enrichment] Đã bắn Bundle tối giản cho {QuoteCode}.", quote.Code.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ">>> [AI-Enrichment] Lỗi khi xử lý báo giá {QuoteId}.", eventData.QuoteId);
            throw;
        }
    }
}
