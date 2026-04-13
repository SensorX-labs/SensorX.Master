using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.TransferOrderAggregate;

public class TransferOrderItem : Entity<TransferOrderItemId>
{
    public ProductId ProductId { get; private set; } = null!;
    public Code ProductCode { get; private set; } = null!;
    public string ProductName { get; private set; } = null!;
    public string Unit { get; private set; } = null!;
    public Quantity Quantity { get; private set; } = null!;
    public string ManufactureName { get; set; } = null!;
    public string Note { get; set; } = null!;

    private TransferOrderItem() : base() { }

    public TransferOrderItem(TransferOrderItemId id, ProductId productId, Code productCode, string productName, string unit, Quantity quantity, string manufactureName, string note) : base(id)
    {
        ProductId = productId;
        ProductCode = productCode;
        ProductName = productName;
        Unit = unit;
        Quantity = quantity;
        ManufactureName = manufactureName;
        Note = note;
    }
}
