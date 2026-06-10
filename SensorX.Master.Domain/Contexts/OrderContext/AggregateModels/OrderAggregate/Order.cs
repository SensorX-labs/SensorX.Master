using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;
using SensorX.Master.Domain.Events;
using System.Text.Json.Serialization;

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
    
    [JsonInclude]
    private readonly List<OrderItem> _items = new();
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    private Order() : base() { }

    [JsonConstructor]
    public Order(OrderId id, QuoteId quoteId, Code code, CustomerId customerId, DeliveryInfo deliveryInfo, SenderInfo senderInfo, OrderStatus status, DateTimeOffset orderDate, IReadOnlyList<OrderItem>? items = null) : base(id)
    {
        QuoteId = quoteId;
        Code = code;
        CustomerId = customerId;
        DeliveryInfo = deliveryInfo;
        SenderInfo = senderInfo;
        Status = status;
        OrderDate = orderDate;
        if (items != null)
        {
            _items.AddRange(items);
        }
    }

    public void AddItem(OrderItem item)
    {
        _items.Add(item);
    }
    // Event tạo order
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
        if (Status == OrderStatus.Cancelled) return;
        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void StartProcessing()
    {
        if (Status == OrderStatus.Processing) return;
        
        if (Status == OrderStatus.PendingPayment)
        {
            Status = OrderStatus.Processing;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void Dispatch()
    {
        if (Status == OrderStatus.Dispatched) return;
        Status = OrderStatus.Dispatched;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

