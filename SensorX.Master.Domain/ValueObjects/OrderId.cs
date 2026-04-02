using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.ValueObjects;

public record OrderId(Guid Value) : EntityId<OrderId>(Value);