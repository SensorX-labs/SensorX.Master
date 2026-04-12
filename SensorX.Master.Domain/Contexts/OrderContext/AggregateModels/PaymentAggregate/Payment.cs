using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.PaymentAggregate;

public class Payment : Entity<PaymentId>
{
    public InvoiceId InvoiceId { get; init; }
    public OrderId OrderId { get; init; }
    public Money Amount { get; init; }
    public PaymentMethod Method { get; init; }
    public PaymentStatus Status { get; init; }
    public DateTimeOffset TransactionDate { get; init; }
    public string BankTransactionId { get; init; }
    public string TransferContent { get; init; }

    public void MarkAsCompleted()
    {
        Status = PaymentStatus.Completed;
    }

    public void MarkAsFailed(string reason)
    {
        Status = PaymentStatus.Failed;
    }
}