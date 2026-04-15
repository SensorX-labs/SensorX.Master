using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
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
        (
            "Nguyễn Văn A",
            Phone.Create("0123456789"),
            "Company",
            Email.From("test@test.com"),
            "Hà Nội",
            "123456"
        );

        var senderInfo = new SenderInfo
        {
            Name = "Sender Name",
            Email = Email.From("sender@test.com")
        };

        var order = new Order(
            new OrderId(Guid.NewGuid()),
            new QuoteId(Guid.NewGuid()),
            Code.Create("ORD"),
            new CustomerId(Guid.NewGuid()),
            customerInfo,
            senderInfo,
            OrderStatus.PendingPayment,
            DateTimeOffset.Now
        );

        var item1 = new OrderItem(
            new OrderItemId(Guid.NewGuid()),
            new ProductId(Guid.NewGuid()),
            Code.Create("PROD"),
            "Product 1",
            "Manufacturer 1",
            "piece",
            new Quantity(2),
            Money.FromVnd(100000),
            Percent.From(10),
            null
        );

        var item2 = new OrderItem(
            new OrderItemId(Guid.NewGuid()),
            new ProductId(Guid.NewGuid()),
            Code.Create("PROD"),
            "Product 2",
            "Manufacturer 2",
            "piece",
            new Quantity(1),
            Money.FromVnd(50000),
            Percent.From(0),
            null
        );

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
        var customerInfo = new CustomerInfo
        (
            "Test",
            Phone.Create("0123456789"),
            "Test Company",
            Email.From("a@b.com"),
            "Test Address",
            "123456"
        );

        var senderInfo = new SenderInfo
        {
            Name = "Sender",
            Email = Email.From("sender@test.com")
        };

        var order = new Order(
            new OrderId(Guid.NewGuid()),
            new QuoteId(Guid.NewGuid()),
            Code.Create("ORD"),
            new CustomerId(Guid.NewGuid()),
            customerInfo,
            senderInfo,
            OrderStatus.Processing,
            DateTimeOffset.Now
        );

        var item = new OrderItem(
            new OrderItemId(Guid.NewGuid()),
            new ProductId(Guid.NewGuid()),
            Code.Create("PROD"),
            "Product 1",
            "Manufacturer 1",
            "piece",
            new Quantity(1),
            Money.FromVnd(100000),
            Percent.From(10),
            null
        );

        order.AddItem(item);

        return order;
    }
}
