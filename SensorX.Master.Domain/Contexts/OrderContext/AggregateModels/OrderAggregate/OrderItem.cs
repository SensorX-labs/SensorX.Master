using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;

public class OrderItem
{
    public OrderItemId Id { get; private set; }
    public ProductId ProductId { get; private set; }
    public Code ProductCode { get; private set; }
    public string ProductName { get; private set; }
    public string Manufacturer { get; private set; }
    public string Unit { get; private set; }
    public Quantity Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    public Percent TaxRate { get; private set; }
    public string? Note { get; private set; }

    public Money GetLineAmount() => UnitPrice * Quantity;
    public Money GetTaxAmount() => GetLineAmount() * TaxRate;
    public Money GetTotalLineAmount() => GetLineAmount() + GetTaxAmount();
}

