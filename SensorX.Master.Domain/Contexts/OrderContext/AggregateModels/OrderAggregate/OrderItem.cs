using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;

public class OrderItem
{
    public OrderItemId Id { get; init; }
    public ProductId ProductId { get; init; }
    public Code ProductCode { get; init; }
    public string ProductName { get; init; }
    public string Manufacturer { get; init; }
    public string Unit { get; init; }
    public Quantity Quantity { get; init; }
    public Money UnitPrice { get; init; }
    public Percent TaxRate { get; init; }
    public string? Note { get; init; }

    public Money GetLineAmount() => UnitPrice * Quantity;
    public Money GetTaxAmount() => GetLineAmount() * TaxRate;
    public Money GetTotalLineAmount() => GetLineAmount() + GetTaxAmount();
}
