using MediatR;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Application.UseCases.Orders.Commands.CreateOrder;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.Events;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.Events.DomainEvents.CustomerRespondedQuote;

public class RFQCancelledEventHandler(
    IRepository<RFQ> _quoteRepository
) : INotificationHandler<DomainEventNotification<CustomerRespondedQuoteEvent>>
{
    public async Task Handle(
        DomainEventNotification<CustomerRespondedQuoteEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        if (domainEvent.QuoteResponse.ResponseType != QuoteResponseStatus.Declined) return;

        var rfq = await _quoteRepository.GetByIdAsync(domainEvent.QuoteId, cancellationToken);
        if (rfq is null) return;

        rfq.Cancel();
        await _quoteRepository.SaveChangesAsync(cancellationToken);
    }
}
