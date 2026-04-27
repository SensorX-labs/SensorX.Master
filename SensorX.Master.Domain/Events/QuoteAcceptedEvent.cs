using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.Events
{
    public record QuoteAcceptedEvent(Guid QuoteId) : IDomainEvent
    {
        public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
    }
}
