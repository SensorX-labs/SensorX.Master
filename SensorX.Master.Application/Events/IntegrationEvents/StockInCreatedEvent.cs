using MassTransit;

namespace SensorX.Master.Application.Events.IntegrationEvents;

[MessageUrn("stock-in-created")]
public record StockInCreatedEvent
{
    public Guid StockInId { get; init; }
    public string TransferOrderCode { get; init; } = string.Empty;
}
