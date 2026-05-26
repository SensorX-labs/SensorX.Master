using MassTransit;

namespace SensorX.Master.Application.Events.IntegrationEvents;

[MessageUrn("supply-request-created")]
public record SupplyRequestCreatedEvent
{
    public Guid SupplyRequestId { get; init; }
    public Guid PickingNoteId { get; init; }
    public Guid WarehouseId { get; init; }
}
