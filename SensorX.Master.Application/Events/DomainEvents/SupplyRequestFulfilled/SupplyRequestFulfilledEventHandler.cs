using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Application.Events.IntegrationEvents;
using SensorX.Master.Domain.Events;

namespace SensorX.Master.Application.Events.DomainEvents.SupplyRequestFulfilled;

public class SupplyRequestFulfilledEventHandler(
    ILogger<SupplyRequestFulfilledEventHandler> logger,
    IPublishEndpoint publishEndpoint
) : INotificationHandler<DomainEventNotification<SupplyRequestFulfilledDomainEvent>>
{
    public async Task Handle(DomainEventNotification<SupplyRequestFulfilledDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        
        logger.LogInformation("Master publishing SupplyRequestFulfilledEvent for SupplyRequest {SupplyRequestId}", domainEvent.SupplyRequestId);

        await publishEndpoint.Publish(new SupplyRequestFulfilledEvent
        {
            SupplyRequestId = domainEvent.SupplyRequestId,
            PickingNoteId = domainEvent.PickingNoteId,
            WarehouseId = domainEvent.WarehouseId
        }, cancellationToken);
    }
}
