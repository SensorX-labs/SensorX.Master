namespace SensorX.Master.Application.Events.DomainEvents.OrderCreated;

using MediatR;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.Events;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

public class RFQConvertedEventHandler(
    IRepository<RFQ> _rfqRepository,
    IQueryBuilder<Order> _orderBuilder,
    IQueryBuilder<Quote> _quoteBuilder,
    IQueryExecutor _queryExecutor
) : INotificationHandler<DomainEventNotification<OrderCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<OrderCreatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        var query = from o in _orderBuilder.QueryAsNoTracking
                    join q in _quoteBuilder.QueryAsNoTracking on o.QuoteId equals q.Id
                    where o.Id == domainEvent.OrderId
                    select q.RFQId;

        var rfqId = await _queryExecutor.FirstOrDefaultAsync(query, cancellationToken);

        var rfq = await _rfqRepository.GetByIdAsync(rfqId, cancellationToken);
        if (rfq is null) return;

        rfq.Cancel();
        await _rfqRepository.SaveChangesAsync(cancellationToken);
    }
}