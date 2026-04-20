using System;

namespace SensorX.Master.Application.IntegrationEvents
{
    public record QuoteCreatedIntegrationEvent
    {
        public Guid QuoteId { get; init; }
        public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
    }
}
