using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate
{
    public record QuoteItemId(Guid Value) : EntityId<QuoteItemId>(Value);
}
