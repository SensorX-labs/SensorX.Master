using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;

public class InvoiceItem
{
    public InvoiceItemId Id { get; init; }
    public ProductId ProductId { get; init; }
    public string ProductName { get; init; }
    public string Unit { get; init; }
    public Quantity Quantity { get; init; }
    public Money UnitPrice { get; init; }
    public Percent TaxRate { get; init; }
    public Money LineAmount { get; init; }
    public Money TaxAmount { get; init; }
    public Money TotalLineAmount { get; init; }

    public static InvoiceItem Create(OrderItemId orderItemId, ProductId productId, string productName, string unit, Quantity quantity, Money unitPrice, Percent taxRate)
    {
        return new InvoiceItem
        {
            Id = orderItemId,
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