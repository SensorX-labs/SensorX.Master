using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate
{
    public record RFQId(Guid Value) : EntityId<RFQId>(Value);
}
