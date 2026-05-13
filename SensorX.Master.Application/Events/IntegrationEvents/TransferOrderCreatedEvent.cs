using MassTransit;

namespace SensorX.Master.Application.Events.IntegrationEvents;

[MessageUrn("transfer-order-created")]
public record TransferOrderCreatedEvent
{
    public Guid TransferOrderId { get; init; }
    public Guid FromWarehouseId { get; init; }
    public string TransferOrderCode { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}
