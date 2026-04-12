using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;

public class CustomerInfo
{
    public string RecipientName { get; init; }
    public string RecipientPhone { get; init; }
    public string CompanyName { get; init; }
    public Email Email { get; init; }
    public string Address { get; init; }
    public string TaxCode { get; init; }
}