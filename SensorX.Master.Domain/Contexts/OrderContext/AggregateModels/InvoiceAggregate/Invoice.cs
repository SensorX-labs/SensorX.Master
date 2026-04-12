using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;

public class Invoice : Entity<InvoiceId>
{
    public Code Code { get; set; }
    public OrderId OrderId { get; set; }
    public BillingInfo BillingInfo { get; set; }
    public string InvoiceSymbol { get; set; }
    public string InvoiceNumber { get; set; }
    public string TaxAuthorityCode { get; set; }
    public DateTimeOffset IssueAt { get; set; }
    public Money SubTotal { get; set; }
    public Money TaxAmount { get; set; }
    public Money GrandTotal { get; set; }
    public Money AmountPaid { get; set; }
    public InvoiceStatus Status { get; set; }
    private readonly List<InvoiceItem> _items = new();
    public IReadOnlyList<InvoiceItem> Items => _items.AsReadOnly();

    public static Invoice Create(OrderId orderId, BillingInfo billingInfo, Money subTotal, Money taxAmount)
    {
        return new Invoice
        {
            Id = InvoiceId.New(),
            OrderId = orderId,
            BillingInfo = billingInfo,
            SubTotal = subTotal,
            TaxAmount = taxAmount,
            GrandTotal = subTotal + taxAmount,
            AmountPaid = Money.Zero("VND"),
            Status = InvoiceStatus.Unpaid
        };
    }

    public void AddItem(InvoiceItem item)
    {
        _items.Add(item);
    }

    public string GetExpectedTransferSyntax()
    {
        return $"{OrderId.Value}|{GrandTotal.Amount}|{BillingInfo.TaxCode}";
    }

    public void RecordPayment(Money amount)
    {
        AmountPaid += amount;
        if (AmountPaid >= GrandTotal)
        {
            Status = InvoiceStatus.Paid;
        }
        else
        {
            Status = InvoiceStatus.PartiallyPaid;
        }
    }

    public void Issue(string taxAuthorityCode)
    {
        TaxAuthorityCode = taxAuthorityCode;
        Status = InvoiceStatus.Issued;
    }
}
