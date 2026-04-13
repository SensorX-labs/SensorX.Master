using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.StrongIDs;

public record SupplyRequestId(Guid Value) : EntityId<SupplyRequestId>(Value);
