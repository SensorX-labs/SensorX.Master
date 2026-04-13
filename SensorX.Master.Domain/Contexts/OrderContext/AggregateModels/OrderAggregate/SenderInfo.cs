using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;

public record SenderInfo
{
    public required string Name { get; init; }
    public required Email Email { get; init; }
}
