using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;
using SensorX.Master.Domain.SeedWork;


namespace SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.TransferOrderAggregate;

public class TransferOrderItem : Entity<TransferOrderItemId>
{
    public TransferOrderItemId Id { get; set; }
    public ProductId ProductId { get; set; }
    public Code ProductCode { get; set; }
    public string ProductName { get; set; }
    public string Unit { get; set; }
    public Quantity Quantity { get; set; }
    public string ManufactureName { get; set; }
    public string Note { get; set; }
}
