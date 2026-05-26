using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
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
    IWarehouseQueryService warehouseQueryService,
    IGeolocationQueryService geolocationQueryService,
    SensorX.Master.Application.Common.Interfaces.IQueryBuilder<Invoice> _invoiceBuilder,
    SensorX.Master.Application.Common.Interfaces.IQueryBuilder<Payment> _paymentBuilder,
    SensorX.Master.Application.Common.Interfaces.IQueryExecutor _queryExecutor
) : INotificationHandler<DomainEventNotification<OrderCreatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<OrderCreatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        // Idempotency: if invoice already exists for this order, skip processing
        var orderId = new SensorX.Master.Domain.StrongIDs.OrderId(domainEvent.OrderId);

        var invoiceQuery = from i in _invoiceBuilder.QueryAsNoTracking
                   where i.OrderId == orderId
                   select i;

        var alreadyExists = await _queryExecutor.AnyAsync(invoiceQuery, cancellationToken);
        if (alreadyExists)
        {
            logger.LogInformation("Invoice already exists for Order {OrderId}, skipping creation.", domainEvent.OrderId);
            return;
        }

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

        var paymentQuery = from p in _paymentBuilder.QueryAsNoTracking
                   where p.OrderId == orderId
                   select p;

        var paymentExists = await _queryExecutor.AnyAsync(paymentQuery, cancellationToken);

        var payment = orderService.CreatePaymentForInvoice(invoice, inventoryRows);

        await invoiceRepository.Add(invoice, cancellationToken);

        if (!paymentExists)
        {
            await paymentRepository.Add(payment, cancellationToken);
        }
        else
        {
            logger.LogInformation("Payment already exists for Order {OrderId}, skipping creation.", domainEvent.OrderId);
        }

        logger.LogInformation(
            "Invoice created from order: {OrderId} -> {InvoiceId}",
            domainEvent.OrderId,
            invoice.Id.Value);

        // Determine nearest warehouse by geocoding the delivery address
        Guid? assignedWarehouseId = null;
        try
        {
            var geos = await geolocationQueryService.GetGeolocationByAddress(domainEvent.Address, cancellationToken);
            var geo = geos?.FirstOrDefault();
            if (geo != null)
            {
                var nearest = await warehouseQueryService.FindNearestWarehouseAsync(geo.Latitude, geo.Longitude, cancellationToken);
                if (nearest != null)
                {
                    assignedWarehouseId = nearest.Id;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to determine nearest warehouse for Order {OrderId}", domainEvent.OrderId);
        }

        await publishEndpoint.Publish(new OrderCreatedEvent
        {
            OrderId = domainEvent.OrderId,
            OrderCode = domainEvent.OrderCode,
            CreatedAt = DateTimeOffset.UtcNow,
            ReceiverName = domainEvent.RecipientName,
            ReceiverPhone = domainEvent.RecipientPhone,
            DeliveryAddress = domainEvent.Address,
            CompanyName = domainEvent.CompanyName,
            TaxCode = domainEvent.TaxCode,
            AssignedWarehouseId = assignedWarehouseId
        }, cancellationToken);
    }
}