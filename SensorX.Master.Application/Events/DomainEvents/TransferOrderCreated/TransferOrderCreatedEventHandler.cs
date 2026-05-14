
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Domain.Events;
using SensorX.Master.Application.Events.IntegrationEvents;

namespace SensorX.Master.Application.Events.DomainEvents.TransferOrderCreated;

public class TransferOrderCreatedEventHandler(
    ILogger<TransferOrderCreatedEventHandler> logger,
    IPublishEndpoint publishEndpoint
) : INotificationHandler<DomainEventNotification<TransferOrderCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<TransferOrderCreatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        logger.LogInformation("Master project publishing TransferOrderCreatedEvent: {TransferOrderId}", domainEvent.TransferOrderId);

        await publishEndpoint.Publish(new TransferOrderCreatedEvent
        {
            TransferOrderId = domainEvent.TransferOrderId,
            FromWarehouseId = domainEvent.FromWarehouseId,
            TransferOrderCode = domainEvent.TransferOrderCode,
            CreatedAt = DateTimeOffset.UtcNow
        }, cancellationToken);
    }
}
