using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.PaymentAggregate;

public class Payment : Entity<PaymentId>
{
    public InvoiceId InvoiceId { get; set; }
    public OrderId OrderId { get; set; }
    public Money Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTimeOffset TransactionDate { get; set; }
    public string BankTransactionId { get; set; }
    public string TransferContent { get; set; }

    public void MarkAsCompleted()
    {
        Status = PaymentStatus.Completed;
    }

    public void MarkAsFailed(string reason)
    {
        Status = PaymentStatus.Failed;
    }
}
