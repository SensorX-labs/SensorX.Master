using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.SupplyRequestAggregate;

public class SupplyRequest : Entity<SupplyRequestId> , ICreationTrackable , IUpdateTrackable
{
    public WarehouseId WarehouseId { get; private set; } = null!;
    public SupplyRequestStatus Status { get; private set; }
    public string Note { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    private readonly List<SupplyRequestItem> _items = new();
    public IReadOnlyList<SupplyRequestItem> Items => _items.AsReadOnly();

    private readonly List<PurchaseOption> _purchaseOptions = new();
    public IReadOnlyList<PurchaseOption> PurchaseOptions => _purchaseOptions.AsReadOnly();

    private SupplyRequest() : base() { }

    public SupplyRequest(SupplyRequestId id, WarehouseId warehouseId, SupplyRequestStatus status, string note) : base(id)
    {
        WarehouseId = warehouseId;
        Status = status;
        Note = note;
    }

    public void Complete()
    {
        Status = SupplyRequestStatus.Completed;
    }

    public void AddItem(ProductId productId, Quantity requestedQuantity)
    {
        _items.Add(new SupplyRequestItem(SupplyRequestItemId.New(), productId, requestedQuantity));
    }

    public void AddPurchaseOption(ProductId productId, Quantity quantity, string note)
    {
        _purchaseOptions.Add(new PurchaseOption(PurchaseOptionId.New(), productId, quantity, note));
    }
}
