using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.TransferOrderAggregate;

public class TransferOrder : Entity<TransferOrderId>
{
    public Code Code { get; private set; } = null!;
    public WarehouseId SourceWarehouseId { get; private set; } = null!;
    public WarehouseId DestinationWarehouseId { get; private set; } = null!;
    public TransferOrderStatus Status { get; private set; }
    public string Note { get; private set; } = null!;
    public SupplyRequestId? SupplyRequestId { get; private set; }

    private readonly List<TransferOrderItem> _items = new();
    public IReadOnlyList<TransferOrderItem> Items => _items.AsReadOnly();

    private TransferOrder() : base() { }

    public TransferOrder(TransferOrderId id, Code code, WarehouseId sourceWarehouseId, WarehouseId destinationWarehouseId, TransferOrderStatus status, string note, SupplyRequestId? supplyRequestId = null) : base(id)
    {
        Code = code;
        SourceWarehouseId = sourceWarehouseId;
        DestinationWarehouseId = destinationWarehouseId;
        Status = status;
        Note = note;
        SupplyRequestId = supplyRequestId;
    }

    public void Complete()
    {
        Status = TransferOrderStatus.Completed;
    }

    public void AddItem(ProductId productId, Code productCode, string productName, string unit, Quantity quantity, string manufactureName, string note)
    {
        _items.Add(new TransferOrderItem(TransferOrderItemId.New(), productId, productCode, productName, unit, quantity, manufactureName, note));
    }
}
