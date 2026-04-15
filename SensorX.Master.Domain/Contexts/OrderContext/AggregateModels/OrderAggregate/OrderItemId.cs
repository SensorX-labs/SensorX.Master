using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;

public record OrderItemId(Guid Value) : EntityId<OrderItemId>(Value);
