using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.StrongIDs;

public record CategoryId(Guid Value) : EntityId<CategoryId>(Value);
