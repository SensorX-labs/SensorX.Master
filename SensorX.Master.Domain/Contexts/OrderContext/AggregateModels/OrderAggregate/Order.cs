using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;

public class Order : Entity<OrderId>
{
    public QuoteId QuoteId { get; init; }
    public Code Code { get; init; }
    public CustomerId CustomerId { get; init; }
    public CustomerInfo CustomerInfo { get; init; }
    public SenderInfo SenderInfo { get; init; }
    public OrderStatus Status { get; init; }
    public DateTimeOffset OrderDate { get; init; }
    private readonly List<OrderItem> _items = new();
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    public void AddItem(OrderItem item)
    {
        _items.Add(item);
    }

    public Money GetSubtotal()
    {
        return _items.Sum(item => item.GetLineAmount());
    }

    public Money GetTotalTax()
    {
        return _items.Sum(item => item.GetTaxAmount());
    }

    public Money GetGrandTotal()
    {
        return _items.Sum(item => item.GetTotalLineAmount());
    }
}
