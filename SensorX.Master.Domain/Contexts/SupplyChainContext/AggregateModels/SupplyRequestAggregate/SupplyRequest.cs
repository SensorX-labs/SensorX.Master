using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.SupplyRequestAggregate;

public class SupplyRequest : Entity<SupplyRequestId>
{
    public SupplyRequestId Id { get; private set; }
    public WarehouseId WarehouseId { get; private set; }
    public SupplyRequestStatus Status { get; private set; }
    public string Note { get; private set; }

    private readonly List<SupplyRequestItem> _items = new();
    public IReadOnlyList<SupplyRequestItem> Items => _items.AsReadOnly();

    private readonly List<PurchaseOption> _purchaseOptions = new();
    public IReadOnlyList<PurchaseOption> PurchaseOptions => _purchaseOptions.AsReadOnly();

    public void Complete()
    {
        Status = SupplyRequestStatus.Completed;
    }

    public void AddItem(ProductId productId, Quantity requestedQuantity)
    {
        _items.Add(new SupplyRequestItem
        {
            Id = SupplyRequestItemId.New(),
            ProductId = productId,
            RequestedQuantity = requestedQuantity
        });
    }

    public void AddPurchaseOption(ProductId productId, Quantity quantity, string note)
    {
        _purchaseOptions.Add(new PurchaseOption
        {
            Id = PurchaseOptionId.New(),
            ProductId = productId,
            Quantity = quantity,
            Note = note
        });
    }
}
