using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate
{
    public record RFQItemId(Guid Value) : EntityId<RFQItemId>(Value);
}
