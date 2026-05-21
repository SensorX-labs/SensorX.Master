namespace SensorX.Master.Application.Events.DomainEvents.OrderCreated;

using MediatR;
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
    IQueryExecutor _queryExecutor
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
        if (rfq is null) return;

        rfq.MarkAsConverted();

        await _rfqRepository.SaveChangesAsync(cancellationToken);
    }
}