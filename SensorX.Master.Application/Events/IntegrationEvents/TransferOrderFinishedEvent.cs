using MassTransit;

namespace SensorX.Master.Application.Events.IntegrationEvents;

[MessageUrn("transfer-order-finished")]
public record TransferOrderFinishedEvent
{
    public Guid TransferOrderId { get; init; }
    public Guid PickingNoteId { get; init; }
    public Guid ToWarehouseId { get; init; }
    public DateTimeOffset FinishedAt { get; init; }
}
