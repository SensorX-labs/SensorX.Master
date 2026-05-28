using MassTransit;

namespace SensorX.Master.Application.Events.IntegrationEvents;

[MessageUrn("stock-out-created")]
public record StockOutCreatedEvent
{
    public Guid StockOutId { get; init; }
    public int SourceType { get; init; } // 0 = SalesOrder, 1 = TransferOrder
    public Guid SourceId { get; init; }
}
