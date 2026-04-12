using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;

public class InvoiceItem
{
    public InvoiceItemId Id { get; set; }
    public ProductId ProductId { get; set; }
    public string ProductName { get; set; }
    public string Unit { get; set; }
    public Quantity Quantity { get; set; }
    public Money UnitPrice { get; set; }
    public Percent TaxRate { get; set; }
    public Money LineAmount { get; set; }
    public Money TaxAmount { get; set; }
    public Money TotalLineAmount { get; set; }

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
