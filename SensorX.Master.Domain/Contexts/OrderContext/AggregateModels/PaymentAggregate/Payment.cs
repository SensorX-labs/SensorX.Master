using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.PaymentAggregate;

public class Payment : Entity<PaymentId>
{
    public InvoiceId InvoiceId { get; private set; }
    public OrderId OrderId { get; private set; }
    public Money Amount { get; private set; }
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTimeOffset TransactionDate { get; private set; }
    public string BankTransactionId { get; private set; }
    public string TransferContent { get; private set; }

    private Payment() : base() { }

    public Payment(PaymentId id, InvoiceId invoiceId, OrderId orderId, Money amount, PaymentMethod method, PaymentStatus status, DateTimeOffset transactionDate, string bankTransactionId, string transferContent) : base(id)
    {
        InvoiceId = invoiceId;
        OrderId = orderId;
        Amount = amount;
        Method = method;
        Status = status;
        TransactionDate = transactionDate;
        BankTransactionId = bankTransactionId;
        TransferContent = transferContent;
    }

    public void MarkAsCompleted()
    {
        Status = PaymentStatus.Completed;
    }

    public void MarkAsFailed(string reason)
    {
        Status = PaymentStatus.Failed;
    }
}
