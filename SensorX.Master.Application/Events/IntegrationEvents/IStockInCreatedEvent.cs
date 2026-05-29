using System;
using MassTransit;

namespace SensorX.Master.Application.Events.IntegrationEvents;

[MessageUrn("stock-in-created")]
public interface IStockInCreatedEvent
{
    Guid StockInId { get; }
    string TransferOrderCode { get; }
}
