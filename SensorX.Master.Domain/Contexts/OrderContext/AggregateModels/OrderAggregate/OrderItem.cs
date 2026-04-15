using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;

public class OrderItem : Entity<OrderItemId>
{
    public ProductId ProductId { get; private set; }
    public Code ProductCode { get; private set; }
    public string ProductName { get; private set; }
    public string Manufacturer { get; private set; }
    public string Unit { get; private set; }
    public Quantity Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    public Percent TaxRate { get; private set; }
    public string? Note { get; private set; }

    private OrderItem() : base() { }

    public OrderItem(OrderItemId id, ProductId productId, Code productCode, string productName, string manufacturer, string unit, Quantity quantity, Money unitPrice, Percent taxRate, string? note) : base(id)
    {
        ProductId = productId;
        ProductCode = productCode;
        ProductName = productName;
        Manufacturer = manufacturer;
        Unit = unit;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TaxRate = taxRate;
        Note = note;
    }

    public Money GetLineAmount() => UnitPrice * Quantity;
    public Money GetTaxAmount() => GetLineAmount() * TaxRate;
    public Money GetTotalLineAmount() => GetLineAmount() + GetTaxAmount();
}

