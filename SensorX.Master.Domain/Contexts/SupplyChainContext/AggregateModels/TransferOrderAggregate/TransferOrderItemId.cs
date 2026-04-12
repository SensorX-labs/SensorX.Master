using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.TransferOrderAggregate;

public record TransferOrderItemId(Guid Value) : EntityId<TransferOrderItemId>(Value);
