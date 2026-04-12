using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;

public class BillingInfo
{
    public string CompanyName { get; set; }
    public string TaxCode { get; set; }
    public string Address { get; set; }
    public Email Email { get; set; }
}
