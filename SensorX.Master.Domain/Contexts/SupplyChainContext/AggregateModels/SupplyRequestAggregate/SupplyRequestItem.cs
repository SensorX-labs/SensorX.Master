using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.SupplyRequestAggregate;

public class SupplyRequestItem : Entity<SupplyRequestItemId>
{
    public ProductId ProductId { get; private set; } = null!;
    public Quantity RequestedQuantity { get; private set; } = null!;

    private SupplyRequestItem() : base() { }

    public SupplyRequestItem(SupplyRequestItemId id, ProductId productId, Quantity requestedQuantity) : base(id)
    {
        ProductId = productId;
        RequestedQuantity = requestedQuantity;
    }
}
