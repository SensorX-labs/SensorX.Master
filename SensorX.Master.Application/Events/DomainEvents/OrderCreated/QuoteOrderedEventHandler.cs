namespace SensorX.Master.Application.Events.DomainEvents.OrderCreated;

using MediatR;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.Events;
using SensorX.Master.Domain.SeedWork;

public class QuoteOrderedEventHandler(
    IRepository<Quote> _quoteRepository
) : INotificationHandler<DomainEventNotification<OrderCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<OrderCreatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        var quoteId = domainEvent.Order.QuoteId;
        var quote = await _quoteRepository.GetByIdAsync(quoteId, cancellationToken);
        
        if (quote is null) return;

        // quote.MarkAsOrdered();

        await _quoteRepository.SaveChangesAsync(cancellationToken);
    }
}
