using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;

public class Order : Entity<OrderId>
{
    public QuoteId QuoteId { get; private set; }
    public Code Code { get; private set; }
    public CustomerId CustomerId { get; private set; }
    public CustomerInfo CustomerInfo { get; private set; }
    public SenderInfo SenderInfo { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTimeOffset OrderDate { get; private set; }
    private readonly List<OrderItem> _items = new();
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    public void AddItem(OrderItem item)
    {
        _items.Add(item);
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
}

