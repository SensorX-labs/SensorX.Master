using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;

public class CustomerInfo
{
    public string RecipientName { get; set; }
    public string RecipientPhone { get; set; }
    public string CompanyName { get; set; }
    public Email Email { get; set; }
    public string Address { get; set; }
    public string TaxCode { get; set; }
}
