using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.SupplyRequestAggregate;

public class PurchaseOption : Entity<PurchaseOptionId>
{
    public PurchaseOptionId Id { get; set; }
    public ProductId ProductId { get; set; }
    public Quantity Quantity { get; set; }
    public string Note { get; set; }
}
