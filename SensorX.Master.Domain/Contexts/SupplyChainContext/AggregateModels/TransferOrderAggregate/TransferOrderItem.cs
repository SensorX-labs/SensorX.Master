using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;
using SensorX.Master.Domain.SeedWork;


namespace SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.TransferOrderAggregate;

public class TransferOrderItem : Entity<TransferOrderItemId>
{
    public TransferOrderItemId Id { get; private set; }
    public ProductId ProductId { get; private set; }
    public Code ProductCode { get; private set; }
    public string ProductName { get; private set; }
    public string Unit { get; private set; }
    public Quantity Quantity { get; private set; }
    public string ManufactureName { get; set; }
    public string Note { get; set; }
}
