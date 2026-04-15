
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.StrongIDs;

public record WarehouseId(Guid Value) : EntityId<WarehouseId>(Value);
