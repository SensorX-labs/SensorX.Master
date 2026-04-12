using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.SupplyRequestAggregate;

public record PurchaseOptionId(Guid Value) : EntityId<PurchaseOptionId>(Value);
