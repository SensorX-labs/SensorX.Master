using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.Models.DataServiceModels;
using SensorX.Master.Domain.Events;

namespace SensorX.Master.Application.Events.DomainEvents.Quote.QuoteCreated.QuoteAnlysis;

public sealed class QuoteSubmittedForApprovalEventHandler(
    IQueryBuilder<SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate.Quote> quoteQueryBuilder,
    IQueryBuilder<SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate.RFQ> rfqQueryBuilder,
    IQueryExecutor queryExecutor,
    IDataServiceClient dataClient,
    IPublishEndpoint publishEndpoint,
    ILogger<QuoteSubmittedForApprovalEventHandler> logger
) : INotificationHandler<DomainEventNotification<SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate.QuoteSubmittedForApprovalEvent>>
{
    public async Task Handle(
        DomainEventNotification<SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate.QuoteSubmittedForApprovalEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        try
        {
            var quoteQuery = quoteQueryBuilder.QueryAsNoTracking.Where(x => x.Id == domainEvent.QuoteId);
            var quote = await queryExecutor.FirstOrDefaultAsync(quoteQuery, cancellationToken);
            if (quote is null) return;

            Guid? staffId = null;
            if (quote.RFQId != null)
            {
                var rfqQuery = rfqQueryBuilder.QueryAsNoTracking.Where(x => x.Id == quote.RFQId);
                var rfq = await queryExecutor.FirstOrDefaultAsync(rfqQuery, cancellationToken);
                staffId = rfq?.StaffId?.Value;
            }

            var productIds = quote.LineItems.Select(x => x.ProductId.Value).ToArray();
            var pricingTask = dataClient.GetProductPricingAsync(productIds);
            var staffTask = staffId.HasValue
                ? dataClient.GetEmployeeMetricsAsync(staffId.Value)
                : Task.FromResult(new StaffMetricsApiResponse { IsSuccess = false });

            var allQuotesQuery = quoteQueryBuilder.QueryAsNoTracking
                .Where(x => x.CustomerId == quote.CustomerId && x.Id != quote.Id);
            var customerQuotes = await queryExecutor.ToListAsync(allQuotesQuery, cancellationToken);

            await Task.WhenAll(pricingTask, staffTask);
            var extPricing = await pricingTask;
            var extStaff = await staffTask;

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

            var staffData = extStaff?.Value;
            var (tenureYears, tenureMonths) = CalculateTenure(staffData);

            var bundle = new QuoteAnalysisDataBundle(
                QuoteId: quote.Id.Value.ToString(),
                QuoteCode: quote.Code.Value,
                Customer: new CustomerAnalysisData(
                    CustomerId: quote.CustomerId.Value.ToString(),
                    CompanyName: quote.CustomerInfo.CompanyName,
                    RecipientName: string.Empty,
                    TotalQuotes: customerQuotes.Count,
                    AcceptedQuotes: customerQuotes.Count(q => q.Status == SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate.QuoteStatus.Ordered),
                    RejectedOrExpiredQuotes: customerQuotes.Count(q => q.Status == SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate.QuoteStatus.Returned || q.QuoteDate > DateTimeOffset.UtcNow)
                ),
                Staff: new StaffAnalysisData(
                    StaffId: staffId?.ToString() ?? "N/A",
                    StaffName: staffData?.StaffName ?? "Chưa gán",
                    Department: staffData?.Department ?? "N/A",
                    TenureYears: tenureYears,
                    TenureMonths: tenureMonths
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

            await publishEndpoint.Publish(bundle, cancellationToken);
            logger.LogInformation(">>> [AI-Enrichment] Đã bắn Bundle tối giản cho {QuoteCode}.", quote.Code.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ">>> [AI-Enrichment] Lỗi khi xử lý báo giá {QuoteId}.", domainEvent.QuoteId.Value);
            throw;
        }
    }

    private static (int Years, int Months) CalculateTenure(StaffMetricsData? staffData)
    {
        if (staffData is null)
        {
            return (0, 0);
        }

        var startDate = staffData.JoinDate != default ? staffData.JoinDate : staffData.CreatedAt;
        var now = DateTime.UtcNow;
        var totalMonths = ((now.Year - startDate.Year) * 12) + now.Month - startDate.Month;

        if (now.Day < startDate.Day)
        {
            totalMonths -= 1;
        }

        totalMonths = Math.Max(0, totalMonths);
        return (totalMonths / 12, totalMonths % 12);
    }
}
