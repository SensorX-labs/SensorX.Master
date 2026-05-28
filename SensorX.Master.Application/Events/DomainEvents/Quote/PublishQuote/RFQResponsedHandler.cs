using MediatR;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.Events;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Events.DomainEvents.Quote.PublishQuote;

public sealed class RFQResponsedHandler(
    IRepository<SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate.RFQ> _rfqRepository
) : INotificationHandler<DomainEventNotification<PublishQuoteEvent>>
{
    public async Task Handle(
        DomainEventNotification<PublishQuoteEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        var rfq = await _rfqRepository.GetByIdAsync(domainEvent.RFQId, cancellationToken);
        if (rfq is null) return;

        rfq.MarkAsResponded();
        await _rfqRepository.SaveChangesAsync(cancellationToken);
    }
}