using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.SupplyRequestAggregate;

public class SupplyRequestItem : Entity<SupplyRequestItemId>
{
    public SupplyRequestItemId Id { get; set; }
    public ProductId ProductId { get; set; }
    public Quantity RequestedQuantity { get; set; }
}
