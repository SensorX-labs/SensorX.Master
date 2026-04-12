using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.StrongIDs;

public record OrderId(Guid Value) : EntityId<OrderId>(Value);
