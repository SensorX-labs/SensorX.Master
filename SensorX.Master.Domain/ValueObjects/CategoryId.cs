using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.ValueObjects;

public record CategoryId(Guid Value) : EntityId<CategoryId>(Value);