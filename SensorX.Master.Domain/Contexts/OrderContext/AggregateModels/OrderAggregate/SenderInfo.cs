using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;

public class SenderInfo
{
    public string Name { get; private set; }
    public Email Email { get; private set; }
}
