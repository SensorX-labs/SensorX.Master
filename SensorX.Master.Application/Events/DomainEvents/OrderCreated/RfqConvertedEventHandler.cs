namespace SensorX.Master.Application.Events.DomainEvents.OrderCreated;

using MediatR;
using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.Events;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

public class RFQConvertedEventHandler(
    IRepository<RFQ> _rfqRepository,
    IQueryBuilder<Quote> _quoteBuilder,
    IQueryExecutor _queryExecutor,
    Microsoft.Extensions.Logging.ILogger<RFQConvertedEventHandler> _logger
) : INotificationHandler<DomainEventNotification<OrderCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<OrderCreatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        var quoteId = domainEvent.Order.QuoteId;

        // Use read model to find RFQId without loading Quote aggregate
        var query = from q in _quoteBuilder.QueryAsNoTracking
                    where q.Id == quoteId
                    select q.RFQId;

        var rfqId = await _queryExecutor.FirstOrDefaultAsync(query, cancellationToken);
        if (rfqId is null) return;

        var rfq = await _rfqRepository.GetByIdAsync(rfqId, cancellationToken);
        if (rfq is null)
        {
            _logger.LogWarning("RFQ not found when converting: {RfqId}", rfqId);
            return;
        }

        // Only convert if RFQ is in Responded state (or already Converted).
        // If it's not responded yet, skip conversion to avoid throwing and blocking outbox processing.
        if (rfq.Status != RFQStatus.Responded && rfq.Status != RFQStatus.Converted)
        {
            _logger.LogInformation("Skipping RFQ convert for {RfqId} because status is {Status}", rfqId, rfq.Status);
            return;
        }

        if (rfq.Status == RFQStatus.Converted)
        {
            _logger.LogDebug("RFQ {RfqId} already converted", rfqId);
            return;
        }

        rfq.MarkAsConverted();

        await _rfqRepository.SaveChangesAsync(cancellationToken);
    }
}