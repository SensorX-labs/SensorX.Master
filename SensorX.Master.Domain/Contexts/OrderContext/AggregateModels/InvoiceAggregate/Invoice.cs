using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;

public class Invoice : Entity<InvoiceId>
{
    public Code Code { get; init; }
    public OrderId OrderId { get; init; }
    public BillingInfo BillingInfo { get; init; }
    public string InvoiceSymbol { get; init; }
    public string InvoiceNumber { get; init; }
    public string TaxAuthorityCode { get; init; }
    public DateTimeOffset IssueAt { get; init; }
    public Money SubTotal { get; init; }
    public Money TaxAmount { get; init; }
    public Money GrandTotal { get; init; }
    public Money AmountPaid { get; init; }
    public InvoiceStatus Status { get; init; }
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
            AmountPaid = Money.Zero(Currency.VND),
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