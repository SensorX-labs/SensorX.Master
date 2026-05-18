
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Domain.Events;
using SensorX.Master.Application.Events.IntegrationEvents;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.Services;

namespace SensorX.Master.Application.Events.DomainEvents.OrderCreated;

public class OrderCreatedEventHandler(
    ILogger<OrderCreatedEventHandler> logger,
    IPublishEndpoint publishEndpoint,
    IRepository<Invoice> invoiceRepository,
    OrderService orderService
) : INotificationHandler<DomainEventNotification<OrderCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<OrderCreatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var invoice = orderService.CreateInvoiceFromOrder(domainEvent.Order);

        await invoiceRepository.Add(invoice, cancellationToken);
        logger.LogInformation("Invoice created from order: {OrderId} -> {InvoiceId}", domainEvent.OrderId, invoice.Id.Value);

        logger.LogInformation("Master project publishing OrderCreatedEvent: {OrderId}", domainEvent.OrderId);

        await publishEndpoint.Publish(new OrderCreatedEvent
        {
            OrderId = domainEvent.OrderId,
            OrderCode = domainEvent.OrderCode,
            CreatedAt = DateTimeOffset.UtcNow,
            ReceiverName = domainEvent.RecipientName,
            ReceiverPhone = domainEvent.RecipientPhone,
            DeliveryAddress = domainEvent.Address,
            CompanyName = domainEvent.CompanyName,
            TaxCode = domainEvent.TaxCode
        }, cancellationToken);
    }
}
