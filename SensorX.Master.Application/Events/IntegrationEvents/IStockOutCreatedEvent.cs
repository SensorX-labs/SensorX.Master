using System;
using MassTransit;

namespace SensorX.Master.Application.Events.IntegrationEvents;

[MessageUrn("stock-out-created")]
public interface IStockOutCreatedEvent
{
    Guid StockOutId { get; }
    int SourceType { get; } // 0 = SalesOrder, 1 = TransferOrder
    Guid SourceId { get; }
}
