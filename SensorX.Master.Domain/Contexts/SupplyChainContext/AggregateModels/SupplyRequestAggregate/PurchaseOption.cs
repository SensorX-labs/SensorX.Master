using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.SupplyRequestAggregate;

public class PurchaseOption : Entity<PurchaseOptionId> , ICreationTrackable , IUpdateTrackable
{
    public ProductId ProductId { get; private set; } = null!;
    public Quantity Quantity { get; private set; } = null!;
    public string Note { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    private PurchaseOption() : base() { }

    public PurchaseOption(PurchaseOptionId id, ProductId productId, Quantity quantity, string note) : base(id)
    {
        ProductId = productId;
        Quantity = quantity;
        Note = note;
    }
}
