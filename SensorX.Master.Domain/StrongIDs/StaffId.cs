
using SensorX.Master.Domain.SeedWork;
namespace SensorX.Master.Domain.StrongIDs;

public record StaffId(Guid Value) : EntityId<StaffId>(Value);
