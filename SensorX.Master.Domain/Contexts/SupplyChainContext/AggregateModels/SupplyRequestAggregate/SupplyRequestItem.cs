using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.SupplyRequestAggregate;

public class SupplyRequestItem : Entity<SupplyRequestItemId>
{
    public SupplyRequestItemId Id { get; private set; }
    public ProductId ProductId { get; private set; }
    public Quantity RequestedQuantity { get; private set; }
}
