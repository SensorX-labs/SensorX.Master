using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;

public class SenderInfo
{
    public string Name { get; init; }
    public Email Email { get; init; }
}