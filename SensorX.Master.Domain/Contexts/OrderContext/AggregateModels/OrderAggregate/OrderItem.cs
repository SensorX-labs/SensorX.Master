using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;

public class OrderItem
{
    public OrderItemId Id { get; set; }
    public ProductId ProductId { get; set; }
    public Code ProductCode { get; set; }
    public string ProductName { get; set; }
    public string Manufacturer { get; set; }
    public string Unit { get; set; }
    public Quantity Quantity { get; set; }
    public Money UnitPrice { get; set; }
    public Percent TaxRate { get; set; }
    public string? Note { get; set; }

    public Money GetLineAmount() => UnitPrice * Quantity;
    public Money GetTaxAmount() => GetLineAmount() * TaxRate;
    public Money GetTotalLineAmount() => GetLineAmount() + GetTaxAmount();
}

