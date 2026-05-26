using MassTransit;

namespace SensorX.Master.Application.Events.IntegrationEvents;

[MessageUrn("transfer-order-created")]
public record TransferOrderCreatedEvent
{
    public Guid TransferOrderId { get; init; }
    public Guid PickingNoteId { get; init; }
    public Guid FromWarehouseId { get; init; }
    public Guid ToWarehouseId { get; init; }
    public string TransferOrderCode { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public List<TransferOrderItemDto> Items { get; init; } = new();
}

public record TransferOrderItemDto(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    string Unit,
    decimal Quantity,
    string Manufacturer,
    string Note
);
