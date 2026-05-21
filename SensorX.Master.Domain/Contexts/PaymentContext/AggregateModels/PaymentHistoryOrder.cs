namespace SensorX.Master.Domain.Contexts.PaymentContext.AggregateModels;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
public class PaymentHistoryOrder
{
    public int Id { get; set; }
    public int PaymentHistoryId { get; set; }
    public OrderId OrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentHistory PaymentHistory { get; set; } = null!;
    public Order Order { get; set; } = null!;
}