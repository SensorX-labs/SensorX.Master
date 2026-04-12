using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.StrongIDs;

public record TransferOrderId(Guid Value) : EntityId<TransferOrderId>(Value);
