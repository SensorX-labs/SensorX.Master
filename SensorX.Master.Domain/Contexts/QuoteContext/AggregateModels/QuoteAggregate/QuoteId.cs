using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate
{
    public record QuoteId(Guid Value) : EntityId<QuoteId>(Value);
}
