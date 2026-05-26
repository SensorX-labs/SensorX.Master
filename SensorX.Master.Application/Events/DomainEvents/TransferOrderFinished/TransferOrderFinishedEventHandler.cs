using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Application.Events.IntegrationEvents;
using SensorX.Master.Domain.Events;

namespace SensorX.Master.Application.Events.DomainEvents.TransferOrderFinished;

public class TransferOrderFinishedEventHandler(
    ILogger<TransferOrderFinishedEventHandler> logger,
    IPublishEndpoint publishEndpoint
) : INotificationHandler<DomainEventNotification<TransferOrderFinishedDomainEvent>>
{
    public async Task Handle(DomainEventNotification<TransferOrderFinishedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        
        logger.LogInformation("Master publishing TransferOrderFinishedEvent for TransferOrder {TransferOrderId}", domainEvent.TransferOrderId);

        await publishEndpoint.Publish(new TransferOrderFinishedEvent
        {
            TransferOrderId = domainEvent.TransferOrderId,
            PickingNoteId = domainEvent.PickingNoteId,
            ToWarehouseId = domainEvent.ToWarehouseId,
            FinishedAt = DateTimeOffset.UtcNow
        }, cancellationToken);
    }
}
