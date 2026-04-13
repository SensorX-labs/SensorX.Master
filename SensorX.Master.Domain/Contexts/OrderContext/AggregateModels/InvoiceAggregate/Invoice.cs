using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;

public class Invoice : Entity<InvoiceId>
{
    public Code Code { get; private set; }
    public OrderId OrderId { get; private set; }
    public BillingInfo BillingInfo { get; private set; }
    public string InvoiceSymbol { get; private set; }
    public string InvoiceNumber { get; private set; }
    public string TaxAuthorityCode { get; private set; }
    public DateTimeOffset IssueAt { get; private set; }
    public Money SubTotal { get; private set; }
    public Money TaxAmount { get; private set; }
    public Money GrandTotal { get; private set; }
    public Money AmountPaid { get; private set; }
    public InvoiceStatus Status { get; private set; }
    private readonly List<InvoiceItem> _items = new();
    public IReadOnlyList<InvoiceItem> Items => _items.AsReadOnly();

    private Invoice() : base() { }

    public Invoice(InvoiceId id, Code code, OrderId orderId, BillingInfo billingInfo, string invoiceSymbol, string invoiceNumber, string taxAuthorityCode, DateTimeOffset issueAt, Money subTotal, Money taxAmount, Money grandTotal, Money amountPaid, InvoiceStatus status) : base(id)
    {
        Code = code;
        OrderId = orderId;
        BillingInfo = billingInfo;
        InvoiceSymbol = invoiceSymbol;
        InvoiceNumber = invoiceNumber;
        TaxAuthorityCode = taxAuthorityCode;
        IssueAt = issueAt;
        SubTotal = subTotal;
        TaxAmount = taxAmount;
        GrandTotal = grandTotal;
        AmountPaid = amountPaid;
        Status = status;
    }

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
