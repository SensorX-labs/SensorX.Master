using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;

public record BillingInfo
{
    public required string CompanyName { get; init; }
    public required string TaxCode { get; init; }
    public required string Address { get; init; }
    public required Email Email { get; init; }
}
