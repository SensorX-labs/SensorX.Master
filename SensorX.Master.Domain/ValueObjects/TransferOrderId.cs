using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.ValueObjects;

public record TransferOrderId(Guid Value) : EntityId<TransferOrderId>(Value);