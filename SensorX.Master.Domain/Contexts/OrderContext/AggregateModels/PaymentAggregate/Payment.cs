using SensorX.Master.Domain.Contexts.PaymentContext.AggregateModels;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.PaymentAggregate;

public class Payment : Entity<PaymentId>, IAggregateRoot, ICreationTrackable, IUpdateTrackable
{
    public OrderId OrderId { get; private set; } = new(Guid.Empty);
    public Money Amount { get; private set; } = Money.Zero("VND");
    public PaymentMethod Method { get; private set; } = PaymentMethod.Other;
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public PaymentType PaymentType { get; private set; } = PaymentType.All;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public List<string> PaymentQRURls { get; private set; } = [];

    private Payment() : base() { }

    public Payment(PaymentId id, OrderId orderId, Money amount, PaymentMethod method, PaymentStatus status, PaymentType paymentType) : base(id)
    {
        OrderId = orderId;
        Amount = amount;
        Method = method;
        Status = status;
        PaymentType = paymentType;
    }

    public void SetPaymentType(PaymentType paymentType)
    {
        PaymentType = paymentType;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetQRUrls(List<string> qrUrls)
    {
        PaymentQRURls = qrUrls ?? [];
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsPartiallyPaid()
    {
        Status = PaymentStatus.PartiallyPaid;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsCompleted()
    {
        Status = PaymentStatus.Completed;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsFailed(string reason)
    {
        Status = PaymentStatus.Failed;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reconcile(decimal totalReceived)
    {
        if (totalReceived <= 0)
        {
            return;
        }

        if (totalReceived >= Amount.Amount)
        {
            MarkAsCompleted();
            return;
        }

        MarkAsPartiallyPaid();
    }
    public void Cancel()
    {
        Status = PaymentStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
