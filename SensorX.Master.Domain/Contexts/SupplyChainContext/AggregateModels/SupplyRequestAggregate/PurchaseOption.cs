using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.SupplyRequestAggregate;

public class PurchaseOption : Entity<PurchaseOptionId>  
{
    public ProductId ProductId { get; private set; } = null!;
    public Quantity Quantity { get; private set; } = null!;
    public string Note { get; private set; } = null!;

    private PurchaseOption() : base() { }

    public PurchaseOption(PurchaseOptionId id, ProductId productId, Quantity quantity, string note) : base(id)
    {
        ProductId = productId;
        Quantity = quantity;
        Note = note;
    }
}
