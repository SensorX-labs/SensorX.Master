using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.Services;
using SensorX.Master.Domain.ValueObjects;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Domain.Tests.Contexts.OrderContext;

public class OrderContextTests
{
    private readonly OrderService _orderService = new();

    [Fact]
    public void Order_ShouldCalculateCorrectTotals()
    {
        // Arrange
        var customerInfo = new CustomerInfo
        {
            RecipientName = "Nguyễn Văn A",
            Email = Email.From("test@test.com"),
            Address = "Hà Nội"
        };
        
        var order = new Order
        {
            Id = new OrderId(Guid.NewGuid()),
            Code = Code.Create("ORD"),
            CustomerInfo = customerInfo,
            Status = OrderStatus.PendingPayment,
            OrderDate = DateTimeOffset.Now
        };

        var item1 = new OrderItem
        {
            Id = new OrderItemId(Guid.NewGuid()),
            ProductId = new ProductId(Guid.NewGuid()),
            UnitPrice = Money.FromVnd(100000),
            Quantity = new Quantity(2),
            TaxRate = Percent.From(10) // 10%
        };

        var item2 = new OrderItem
        {
            Id = new OrderItemId(Guid.NewGuid()),
            ProductId = new ProductId(Guid.NewGuid()),
            UnitPrice = Money.FromVnd(50000),
            Quantity = new Quantity(1),
            TaxRate = Percent.From(0) // 0%
        };

        // Act
        order.AddItem(item1);
        order.AddItem(item2);

        // Assert
        // Item 1: 100k * 2 = 200k. Tax 10% = 20k. Total = 220k
        // Item 2: 50k * 1 = 50k. Tax 0% = 0k. Total = 50k
        // Subtotal: 200k + 50k = 250k
        // Total Tax: 20k + 0k = 20k
        // Grand Total: 250k + 20k = 270k

        Assert.Equal(250000, order.GetSubtotal().Amount);
        Assert.Equal(20000, order.GetTotalTax().Amount);
        Assert.Equal(270000, order.GetGrandTotal().Amount);
    }

    [Fact]
    public void OrderService_ShouldCreateCorrectInvoiceFromOrder()
    {
        // Arrange
        var order = CreateSampleOrder();
        
        // Act
        var invoice = _orderService.CreateInvoiceFromOrder(order);

        // Assert
        Assert.Equal(order.Id, invoice.OrderId);
        Assert.Equal(order.GetSubtotal(), invoice.SubTotal);
        Assert.Equal(order.GetGrandTotal(), invoice.GrandTotal);
        Assert.Equal(order.Items.Count, invoice.Items.Count);
        Assert.Equal(InvoiceStatus.Unpaid, invoice.Status);
    }

    [Fact]
    public void Invoice_ShouldUpdateStatusWhenPaid()
    {
        // Arrange
        var order = CreateSampleOrder();
        var invoice = _orderService.CreateInvoiceFromOrder(order);
        var grandTotal = invoice.GrandTotal;

        // Act & Assert 1: Partial Payment
        invoice.RecordPayment(Money.FromVnd(grandTotal.Amount / 2));
        Assert.Equal(InvoiceStatus.PartiallyPaid, invoice.Status);

        // Act & Assert 2: Full Payment
        invoice.RecordPayment(Money.FromVnd(grandTotal.Amount / 2));
        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
    }

    private Order CreateSampleOrder()
    {
        var order = new Order
        {
            Id = new OrderId(Guid.NewGuid()),
            Code = Code.Create("ORD"),
            CustomerInfo = new CustomerInfo { RecipientName = "Test", Email = Email.From("a@b.com") },
            Status = OrderStatus.Processing
        };

        order.AddItem(new OrderItem
        {
            Id = new OrderItemId(Guid.NewGuid()),
            ProductId = new ProductId(Guid.NewGuid()),
            UnitPrice = Money.FromVnd(100000),
            Quantity = new Quantity(1),
            TaxRate = Percent.From(10)
        });

        return order;
    }
}
