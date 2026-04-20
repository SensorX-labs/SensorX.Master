using MassTransit;

namespace SensorX.Master.Application.Events.DomainEvents.QuoteCreated;

[MessageUrn("quote-created")]
public interface IQuoteCreatedEvent
{
    Guid QuoteId { get; set; }
    string QuoteCode { get; set; }
}