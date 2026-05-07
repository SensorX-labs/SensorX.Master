namespace SensorX.Master.Application.Events.DomainEvents.QuoteCreated;

public interface IQuoteCreatedEvent
{
    Guid QuoteId { get; set; }
}