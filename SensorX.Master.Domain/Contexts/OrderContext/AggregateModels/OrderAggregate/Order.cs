using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;
using SensorX.Master.Domain.Events;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;

public class Order : Entity<OrderId>, IAggregateRoot, ICreationTrackable, IUpdateTrackable
{
    public QuoteId QuoteId { get; private set; } = null!;
    public Code Code { get; private set; } = null!;
    public CustomerId CustomerId { get; private set; } = null!;
    public DeliveryInfo DeliveryInfo { get; private set; } = null!;
    public SenderInfo SenderInfo { get; private set; } = null!;
    public OrderStatus Status { get; private set; }
    public DateTimeOffset OrderDate { get; private set; }
    private readonly List<OrderItem> _items = new();
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    private Order() : base() { }

    public Order(OrderId id, QuoteId quoteId, Code code, CustomerId customerId, DeliveryInfo deliveryInfo, SenderInfo senderInfo, OrderStatus status, DateTimeOffset orderDate) : base(id)
    {
        QuoteId = quoteId;
        Code = code;
        CustomerId = customerId;
        DeliveryInfo = deliveryInfo;
        SenderInfo = senderInfo;
        Status = status;
        OrderDate = orderDate;
    }

    public void AddItem(OrderItem item)
    {
        _items.Add(item);
    }

    public void RaiseCreatedDomainEvent()
    {
        AddDomainEvent(new OrderCreatedDomainEvent(
            this,
            Id.Value,
            Code.Value,
            DeliveryInfo.RecipientName,
            DeliveryInfo.RecipientPhone.Value,
            DeliveryInfo.ShippingAddress,
            DeliveryInfo.CompanyName,
            DeliveryInfo.TaxCode
        ));
    }

    public Money GetSubtotal()
    {
        return _items.Select(item => item.GetLineAmount()).Aggregate(Money.Zero("VND"), (a, b) => a + b);
    }

    public Money GetTotalTax()
    {
        return _items.Select(item => item.GetTaxAmount()).Aggregate(Money.Zero("VND"), (a, b) => a + b);
    }

    public Money GetGrandTotal()
    {
        return _items.Select(item => item.GetTotalLineAmount()).Aggregate(Money.Zero("VND"), (a, b) => a + b);
    }

    public void Cancel()
    {
        Status = OrderStatus.Cancelled;
    }
}

