using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.SupplyRequestAggregate;

public class PurchaseOption : Entity<PurchaseOptionId>
{
    public PurchaseOptionId Id { get; private set; }
    public ProductId ProductId { get; private set; }
    public Quantity Quantity { get; private set; }
    public string Note { get; private set; }
}
