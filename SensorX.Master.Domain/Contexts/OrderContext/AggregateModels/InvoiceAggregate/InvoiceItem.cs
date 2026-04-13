using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;

public class InvoiceItem
{
    public InvoiceItemId Id { get; private set; }
    public ProductId ProductId { get; private set; }
    public string ProductName { get; private set; }
    public string Unit { get; private set; }
    public Quantity Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    public Percent TaxRate { get; private set; }
    public Money LineAmount { get; private set; }
    public Money TaxAmount { get; private set; }
    public Money TotalLineAmount { get; private set; }

    public static InvoiceItem Create(OrderItemId orderItemId, ProductId productId, string productName, string unit, Quantity quantity, Money unitPrice, Percent taxRate)
    {
        return new InvoiceItem
        {
            Id = new InvoiceItemId(orderItemId.Value),
            ProductId = productId,
            ProductName = productName,
            Unit = unit,
            Quantity = quantity,
            UnitPrice = unitPrice,
            TaxRate = taxRate,
            LineAmount = unitPrice * quantity,
            TaxAmount = unitPrice * quantity * taxRate,
            TotalLineAmount = unitPrice * quantity + unitPrice * quantity * taxRate
        };
    }
}
