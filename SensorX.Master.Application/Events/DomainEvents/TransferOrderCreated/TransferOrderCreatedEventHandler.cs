
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Domain.Events;
using SensorX.Master.Application.Events.IntegrationEvents;

using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.TransferOrderAggregate;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Events.DomainEvents.TransferOrderCreated;

public class TransferOrderCreatedEventHandler(
    ILogger<TransferOrderCreatedEventHandler> logger,
    IPublishEndpoint publishEndpoint,
    IRepository<TransferOrder> transferOrderRepository
) : INotificationHandler<DomainEventNotification<TransferOrderCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<TransferOrderCreatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        logger.LogInformation("Master project publishing TransferOrderCreatedEvent: {TransferOrderId}", domainEvent.TransferOrderId);

        var transferOrder = await transferOrderRepository.GetByIdAsync(new TransferOrderId(domainEvent.TransferOrderId), cancellationToken);

        var items = transferOrder?.Items.Select(i => new TransferOrderItemDto(
            i.ProductId.Value,
            i.ProductCode.Value,
            i.ProductName,
            i.Unit,
            i.Quantity.Value,
            i.ManufactureName,
            i.Note
        )).ToList() ?? new List<TransferOrderItemDto>();

        await publishEndpoint.Publish(new TransferOrderCreatedEvent
        {
            TransferOrderId = domainEvent.TransferOrderId,
            FromWarehouseId = domainEvent.FromWarehouseId,
            ToWarehouseId = domainEvent.ToWarehouseId,
            PickingNoteId = domainEvent.PickingNoteId,
            TransferOrderCode = domainEvent.TransferOrderCode,
            CreatedAt = DateTimeOffset.UtcNow,
            Items = items
        }, cancellationToken);
    }
}
