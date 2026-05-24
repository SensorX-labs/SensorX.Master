using MediatR;
using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Events.DomainEvents.Quote.CustomerRespondedQuote;

public class UpdateAIAfterQuoteRespondedEventHandler(
    IRepository<StaffContextPerformance> _performanceRepository,
    IQueryBuilder<StaffContextPerformance> _performanceBuilder,
    IQueryExecutor _queryExecutor,
    IDataServiceClient _dataServiceClient,
    IRepository<SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate.Quote> _quoteRepository,
    ILogger<UpdateAIAfterQuoteRespondedEventHandler> _logger
) : INotificationHandler<DomainEventNotification<CustomerRespondedQuoteEvent>>
{
    public async Task Handle(DomainEventNotification<CustomerRespondedQuoteEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        bool isSuccess = domainEvent.QuoteResponse.ResponseType == QuoteResponseStatus.Accepted;
        await ProcessQuoteResponse(domainEvent.QuoteId, isSuccess, cancellationToken);
    }

    private async Task ProcessQuoteResponse(QuoteId quoteId, bool isSuccess, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Đang cập nhật ký ức AI sau khi Quote {Id} phản hồi ({Status}).", quoteId.Value, isSuccess ? "Accepted" : "Rejected");

        var quote = await _quoteRepository.GetByIdAsync(quoteId, cancellationToken);
        if (quote is null) return;

        var staffId = quote.SenderInfo.Id; // Lấy ID của SaleStaff tạo Quote
        var categoryIds = quote.LineItems.Select(x => x.CategoryId).Distinct().ToList();

        var query = _performanceBuilder.QueryAsNoTracking.Where(p => p.StaffId == staffId && categoryIds.Contains(p.CategoryId));
        var existingPerformances = await _queryExecutor.ToListAsync(query, cancellationToken);

        foreach (var categoryId in categoryIds)
        {
            var perf = existingPerformances.FirstOrDefault(p => p.CategoryId == categoryId);
            bool isNew = false;
            if (perf == null)
            {
                perf = new StaffContextPerformance
                {

                    StaffId = staffId,

                    CategoryId = categoryId,
                    SuccessCount = 0,
                    FailureCount = 0,
                    TotalMarginAccumulated = 0
                };
                isNew = true;
            }

            if (isSuccess)
            {
                perf.SuccessCount++;
                // Tính margin cho các item thuộc category này (Tổng (Giá Quote - Giá Sàn) * Số lượng)
                foreach (var item in quote.LineItems.Where(i => i.CategoryId == categoryId))
                {
                    var margin = (item.UnitPrice.Amount - item.FloorPrice.Amount) * item.Quantity.Value;
                    perf.TotalMarginAccumulated += (double)margin;
                }
            }
            else
            {
                perf.FailureCount++;
            }

            if (isNew)
            {
                await _performanceRepository.Add(perf, cancellationToken);
            }
            else
            {
                await _performanceRepository.Update(perf, cancellationToken);
            }
        }
        await _performanceRepository.SaveChangesAsync(cancellationToken);
    }
}
