using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;

public record CustomerInfo
{
    public string RecipientName { get; private set; }
    public string RecipientPhone { get; private set; }
    public string CompanyName { get; private set; }
    public Email Email { get; private set; }
    public string Address { get; private set; }
    public string TaxCode { get; private set; }
}
