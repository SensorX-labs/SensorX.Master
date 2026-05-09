
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Domain.Events;
using SensorX.Master.Application.Events.IntegrationEvents;

namespace SensorX.Master.Application.Events.DomainEvents.OrderCreated;

public class OrderCreatedEventHandler(
    ILogger<OrderCreatedEventHandler> logger,
    IPublishEndpoint publishEndpoint
) : INotificationHandler<DomainEventNotification<OrderCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<OrderCreatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        logger.LogInformation("Master project publishing OrderCreatedEvent: {OrderId}", domainEvent.OrderId);

        await publishEndpoint.Publish(new OrderCreatedEvent
        {
            OrderId = domainEvent.OrderId,
            OrderCode = domainEvent.OrderCode,
            CreatedAt = DateTimeOffset.UtcNow,
            ReceiverName = domainEvent.RecipientName,
            ReceiverPhone = domainEvent.RecipientPhone,
            DeliveryAddress = domainEvent.Address,
            CompanyName = domainEvent.CompanyName,
            TaxCode = domainEvent.TaxCode
        }, cancellationToken);
    }
}
