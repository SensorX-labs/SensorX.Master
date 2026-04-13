using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.TransferOrderAggregate;

public class TransferOrder : Entity<TransferOrderId>
{
    public TransferOrderId Id { get; private set; }
    public Code Code { get; private set; }
    public WarehouseId SourceWarehouseId { get; private set; }
    public WarehouseId DestinationWarehouseId { get; private set; }
    public TransferOrderStatus Status { get; private set; }
    public string Note { get; private set; }
    public SupplyRequestId? SupplyRequestId { get; private set; }

    private readonly List<TransferOrderItem> _items = new();
    public IReadOnlyList<TransferOrderItem> Items => _items.AsReadOnly();

    public void Complete()
    {
        Status = TransferOrderStatus.Completed;
    }

    public void AddItem(ProductId productId, Code productCode, string productName, string unit, Quantity quantity, string manufactureName, string note)
    {
        _items.Add(new TransferOrderItem
        {
            Id = TransferOrderItemId.New(),
            ProductId = productId,
            ProductCode = productCode,
            ProductName = productName,
            Unit = unit,
            Quantity = quantity,
            ManufactureName = manufactureName,
            Note = note
        });
    }
}
