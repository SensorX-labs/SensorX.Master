using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;

public record BillingInfo
{
    public string CompanyName { get; private set; }
    public string TaxCode { get; private set; }
    public string Address { get; private set; }
    public Email Email { get; private set; }
}
