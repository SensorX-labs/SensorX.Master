using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;

public class BillingInfo
{
    public string CompanyName { get; init; }
    public string TaxCode { get; init; }
    public string Address { get; init; }
    public Email Email { get; init; }
}