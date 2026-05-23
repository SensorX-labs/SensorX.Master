using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Application.DTOs;
using SensorX.Master.Application.Events.IntegrationEvents;
using SensorX.Master.Application.Services;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.PaymentAggregate;
using SensorX.Master.Domain.Contexts.SupplyChainContext.ReadModels;
using SensorX.Master.Domain.Events;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.Services;

namespace SensorX.Master.Application.Events.DomainEvents.OrderCreated;

public class OrderCreatedEventHandler(
    ILogger<OrderCreatedEventHandler> logger,
    IPublishEndpoint publishEndpoint,
    IRepository<Invoice> invoiceRepository,
    IRepository<Payment> paymentRepository,
    OrderService orderService,
    IWarehouseQueryService warehouseQueryService
) : INotificationHandler<DomainEventNotification<OrderCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<OrderCreatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        var invoice = orderService.CreateInvoiceFromOrder(domainEvent.Order);

        var inventoryDtos = await warehouseQueryService.GetTotalInventoryRowsAsync(cancellationToken);
        var inventoryRows = inventoryDtos
            .Select(x => new WarehouseInventoryProjection
            {
                WarehouseId = x.WarehouseId,
                ProductId = x.ProductId,
                ProductCode = x.ProductCode,
                ProductName = x.ProductName,
                Unit = x.Unit,
                PhysicalQuantity = x.PhysicalQuantity,
                AllocatedQuantity = x.AllocatedQuantity,
                WarehouseName = x.WarehouseName,
                BrandZone = x.BrandZone,
                RackCode = x.RackCode,
                LastSyncAt = x.LastSyncAt
            })
            .ToList();

        var payment = orderService.CreatePaymentForInvoice(invoice, inventoryRows);

        await invoiceRepository.Add(invoice, cancellationToken);
        await paymentRepository.Add(payment, cancellationToken);

        logger.LogInformation(
            "Invoice created from order: {OrderId} -> {InvoiceId}",
            domainEvent.OrderId,
            invoice.Id.Value);

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