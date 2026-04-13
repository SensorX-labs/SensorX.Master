using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;

public record CustomerInfo
{
    public required string RecipientName { get; init; }
    public required string RecipientPhone { get; init; }
    public required string CompanyName { get; init; }
    public required Email Email { get; init; }
    public required string Address { get; init; }
    public required string TaxCode { get; init; }
}
