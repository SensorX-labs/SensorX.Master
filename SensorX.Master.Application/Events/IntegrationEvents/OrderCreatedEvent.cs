using MassTransit;

namespace SensorX.Master.Application.Events.IntegrationEvents;

[MessageUrn("order-created")]
public record OrderCreatedEvent
{
    public Guid OrderId { get; init; }
    public string OrderCode { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    // DeliveryInfo fields
    public string ReceiverName { get; init; } = string.Empty;
    public string ReceiverPhone { get; init; } = string.Empty;
    public string DeliveryAddress { get; init; } = string.Empty;
    public string CompanyName { get; init; } = string.Empty;
    public string TaxCode { get; init; } = string.Empty;
    // Optional: nearest warehouse assigned for fulfillment
    public Guid? AssignedWarehouseId { get; init; }
}
