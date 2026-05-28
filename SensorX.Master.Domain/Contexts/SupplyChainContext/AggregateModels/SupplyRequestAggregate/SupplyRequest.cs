using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.SupplyRequestAggregate;

public class SupplyRequest : Entity<SupplyRequestId> , IAggregateRoot, ICreationTrackable , IUpdateTrackable
{
    public Code Code { get; private set; } = null!;
    public WarehouseId WarehouseId { get; private set; } = null!;
    public SupplyRequestStatus Status { get; private set; }
    public string Note { get; private set; } = null!;
    public Guid? PickingNoteId { get; private set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    private readonly List<SupplyRequestItem> _items = new();
    public IReadOnlyList<SupplyRequestItem> Items => _items.AsReadOnly();

    private readonly List<PurchaseOption> _purchaseOptions = new();
    public IReadOnlyList<PurchaseOption> PurchaseOptions => _purchaseOptions.AsReadOnly();

    private SupplyRequest() : base() { }

    public SupplyRequest(SupplyRequestId id, Code code, WarehouseId warehouseId, SupplyRequestStatus status, string note, Guid? pickingNoteId = null) : base(id)
    {
        Code = code;
        WarehouseId = warehouseId;
        Status = status;
        Note = note;
        PickingNoteId = pickingNoteId;
    }

    public void Complete()
    {
        Status = SupplyRequestStatus.Completed;
        if (PickingNoteId.HasValue)
        {
            AddDomainEvent(new SensorX.Master.Domain.Events.SupplyRequestFulfilledDomainEvent(Id.Value, PickingNoteId.Value, WarehouseId.Value));
        }
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
