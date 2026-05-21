using MassTransit;
using MediatR;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.Events;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Events.DomainEvents.QuoteCreated;

public sealed class RFQResponsedHandler(
    IRepository<RFQ> _rfqRepository
) : INotificationHandler<DomainEventNotification<QuoteCreatedEvent>>
{
    public async Task Handle(
        DomainEventNotification<QuoteCreatedEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        var rfq = await _rfqRepository.GetByIdAsync(domainEvent.RFQId.Value, cancellationToken);
        if (rfq is null) return;

        rfq.MarkAsResponded();
        await _rfqRepository.SaveChangesAsync(cancellationToken);
    }
}