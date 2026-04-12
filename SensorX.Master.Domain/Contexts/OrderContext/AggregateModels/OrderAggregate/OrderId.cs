using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;

public record OrderId(Guid Value) : EntityId<OrderId>(Value);