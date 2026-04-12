using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;

namespace SensorX.Master.Domain.Services;

public class OrderService
{
    public Invoice CreateInvoiceFromOrder(Order order)
    {
        var subTotal = order.GetSubtotal();
        var taxAmount = order.GetTotalTax();
        var billingInfo = new BillingInfo
        {
            CompanyName = order.CustomerInfo.CompanyName,
            TaxCode = order.CustomerInfo.TaxCode,
            Address = order.CustomerInfo.Address,
            Email = order.CustomerInfo.Email
        };
        var invoice = Invoice.Create(order.Id, billingInfo, subTotal, taxAmount);
        foreach (var item in order.Items)
        {
            invoice.AddItem(InvoiceItem.Create(item.Id, item.ProductId, item.ProductName, item.Unit, item.Quantity, item.UnitPrice, item.TaxRate));
        }
        return invoice;
    }

    public void CancelOrderByCustomer(Order order, Invoice invoice)
    {
        order.Status = OrderStatus.Cancelled;
        invoice.Status = InvoiceStatus.Cancelled;
    }
}