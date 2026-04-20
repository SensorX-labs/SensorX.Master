using MassTransit;
using MediatR;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Domain.Events;

namespace SensorX.Master.Application.Events.DomainEvents.QuoteCreated;

public class QuoteCreatedEventHandler(IPublishEndpoint _publishEndpoint)
    : INotificationHandler<DomainEventNotification<QuoteCreatedEvent>>
{
    public async Task Handle(
        DomainEventNotification<QuoteCreatedEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        await _publishEndpoint.Publish<IQuoteCreatedEvent>(domainEvent, cancellationToken);
    }
}