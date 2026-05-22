using MediatR;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate.Specs;
using SensorX.Master.Domain.Events;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Events.DomainEvents.PublishQuote;

public sealed class QuoteDupplicateCancelledHandler(
    IRepository<Quote> _quoteRepository
) : INotificationHandler<DomainEventNotification<PublishQuoteEvent>>
{
    public async Task Handle(
        DomainEventNotification<PublishQuoteEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        var spec = new GetByRfqIdSpec(domainEvent.QuoteId, domainEvent.RFQId);
        var quoteList = await _quoteRepository.ListAsync(spec, cancellationToken);
        foreach (var q in quoteList)
        {
            q.Cancel();
        }
        await _quoteRepository.SaveChangesAsync(cancellationToken);
    }
}