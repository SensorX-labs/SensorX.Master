using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;

public class Invoice : Entity<InvoiceId>
{
    public Code Code { get; private set; } = null!;
    public OrderId OrderId { get; private set; } = null!;
    public BillingInfo BillingInfo { get; private set; } = null!;
    public string InvoiceSymbol { get; private set; } = null!;
    public string InvoiceNumber { get; private set; } = null!;
    public string TaxAuthorityCode { get; private set; } = null!;
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
        return new Invoice(
            InvoiceId.New(),
            Code.Create("INV"),
            orderId,
            billingInfo,
            string.Empty,
            string.Empty,
            string.Empty,
            DateTimeOffset.UtcNow,
            subTotal,
            taxAmount,
            subTotal + taxAmount,
            Money.Zero("VND"),
            InvoiceStatus.Unpaid
        );
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
        AmountPaid = AmountPaid + amount;
        if (AmountPaid >= GrandTotal)
        {
            Status = InvoiceStatus.Paid;
        }
        else
        {
            Status = InvoiceStatus.PartiallyPaid;
        }
    }

    public void Cancel()
    {
        Status = InvoiceStatus.Cancelled;
    }

    public void Issue(string taxAuthorityCode)
    {
        TaxAuthorityCode = taxAuthorityCode;
        Status = InvoiceStatus.Issued;
    }
}
