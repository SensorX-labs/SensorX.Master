using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.Models.DataServiceModels;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.Events;
namespace SensorX.Master.Application.Events.DomainEvents.QuoteCreated.QuoteAnlysis;

public sealed class QuoteCreatedEventHandler(
    IQueryBuilder<Quote> _quoteQueryBuilder,
    IQueryBuilder<RFQ> _rfqQueryBuilder,
    IQueryExecutor _queryExecutor,
    IDataServiceClient _dataClient,
    IPublishEndpoint _publishEndpoint,
    ILogger<QuoteCreatedEventHandler> _logger
) : INotificationHandler<DomainEventNotification<QuoteCreatedEvent>>
{
    public async Task Handle(
        DomainEventNotification<QuoteCreatedEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        try
        {
            // 1. Load báo giá hiện tại
            var quoteQuery = _quoteQueryBuilder.QueryAsNoTracking.Where(x => x.Id == domainEvent.QuoteId);
            var quote = await _queryExecutor.FirstOrDefaultAsync(quoteQuery, cancellationToken);
            if (quote is null) return;

            // 2. Tìm StaffId từ RFQ
            Guid? staffId = null;
            if (quote.RFQId != null)
            {
                var rfqQuery = _rfqQueryBuilder.QueryAsNoTracking.Where(x => x.Id == quote.RFQId);
                var rfq = await _queryExecutor.FirstOrDefaultAsync(rfqQuery, cancellationToken);
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
            var customerQuotes = await _queryExecutor.ToListAsync(allQuotesQuery, cancellationToken);

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
                    RejectedOrExpiredQuotes: customerQuotes.Count(q => q.Status == QuoteStatus.Returned || q.QuoteDate > DateTimeOffset.UtcNow)
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

            await _publishEndpoint.Publish(bundle, cancellationToken);
            _logger.LogInformation(">>> [AI-Enrichment] Đã bắn Bundle tối giản cho {QuoteCode}.", quote.Code.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ">>> [AI-Enrichment] Lỗi khi xử lý báo giá {QuoteId}.", domainEvent.QuoteId.Value);
            throw;
        }
    }
}