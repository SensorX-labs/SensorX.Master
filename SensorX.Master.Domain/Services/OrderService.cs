using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.PaymentAggregate;
using SensorX.Master.Domain.Contexts.PaymentContext.AggregateModels;
using SensorX.Master.Domain.Contexts.SupplyChainContext.ReadModels;

namespace SensorX.Master.Domain.Services;

public class OrderService
{
    public Invoice CreateInvoiceFromOrder(Order order)
    {
        var subTotal = order.GetSubtotal();
        var taxAmount = order.GetTotalTax();
        var billingInfo = new BillingInfo
        {
            CompanyName = order.DeliveryInfo.CompanyName,
            TaxCode = order.DeliveryInfo.TaxCode,
            Address = order.DeliveryInfo.ShippingAddress,
            Email = order.DeliveryInfo.Email
        };
        var invoice = Invoice.Create(order.Id, billingInfo, subTotal, taxAmount);
        foreach (var item in order.Items)
        {
            invoice.AddItem(InvoiceItem.Create(item.Id, item.ProductId, item.ProductName, item.Unit, item.Quantity, item.UnitPrice, item.TaxRate));
        }
        return invoice;
    }
public Payment CreatePaymentForInvoice(
    Invoice invoice,
    IReadOnlyCollection<WarehouseInventoryProjection> inventoryRows)
{
    var availableByProduct = inventoryRows
        .GroupBy(x => x.ProductId)
        .ToDictionary(
            g => g.Key,
            g => g.Sum(x => x.PhysicalQuantity - x.AllocatedQuantity));

    var allItemsAvailable = invoice.Items.All(item =>
        availableByProduct.TryGetValue(item.ProductId.Value, out var available) &&
        available >= item.Quantity.Value);

    var paymentType = allItemsAvailable
        ? PaymentType.All
        : PaymentType.Partial;

    return new Payment(
        PaymentId.New(),
        invoice.OrderId,
        invoice.GrandTotal,
        PaymentMethod.BankTransfer,
        PaymentStatus.Pending,
        paymentType);
}

    public void CancelOrderByCustomer(Order order, Invoice invoice, Payment payment)
    {
        order.Cancel();
        invoice.Cancel();
        payment.Cancel();
    }
}