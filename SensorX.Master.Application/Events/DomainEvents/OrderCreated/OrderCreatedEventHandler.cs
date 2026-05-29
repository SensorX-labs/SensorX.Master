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
        logger.LogInformation("OrderCreatedEventHandler: [START] Processing OrderId={OrderId}, OrderCode={OrderCode}, Address={Address}", domainEvent.OrderId, domainEvent.OrderCode, domainEvent.Address);

        // Idempotency: if invoice already exists for this order, skip processing
        var orderId = new SensorX.Master.Domain.StrongIDs.OrderId(domainEvent.OrderId);

        var invoiceQuery = from i in _invoiceBuilder.QueryAsNoTracking
                   where i.OrderId == orderId
                   select i;

        var alreadyExists = await _queryExecutor.AnyAsync(invoiceQuery, cancellationToken);
        if (alreadyExists)
        {
            logger.LogInformation("OrderCreatedEventHandler: Invoice already exists for Order {OrderId}, skipping creation and return early.", domainEvent.OrderId);
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
            logger.LogInformation("OrderCreatedEventHandler: Payment already exists for Order {OrderId}, skipping creation.", domainEvent.OrderId);
        }

        logger.LogInformation(
            "OrderCreatedEventHandler: Invoice created from order: {OrderId} -> {InvoiceId}",
            domainEvent.OrderId,
            invoice.Id.Value);
            
        // Calculate NearestWarehouseId
        Guid nearestWarehouseId = Guid.Empty;
        logger.LogInformation("OrderCreatedEventHandler: Calculating NearestWarehouseId for Address='{Address}'", domainEvent.Address);
        try
        {
            var geolocations = await geolocationQueryService.GetGeolocationByAddress(domainEvent.Address, cancellationToken);
            logger.LogInformation("OrderCreatedEventHandler: Geocoding retrieved geolocations count: {Count}", geolocations?.Count ?? 0);
            if (geolocations != null && geolocations.Count > 0 && geolocations.First() != null)
            {
                var geo = geolocations.First()!;
                logger.LogInformation("OrderCreatedEventHandler: Found coordinate Lat={Latitude}, Lon={Longitude} for address", geo.Latitude, geo.Longitude);
                var nearestWarehouse = await warehouseQueryService.FindNearestWarehouseAsync(geo.Latitude, geo.Longitude, cancellationToken);
                if (nearestWarehouse != null)
                {
                    nearestWarehouseId = nearestWarehouse.Id;
                    logger.LogInformation("OrderCreatedEventHandler: Found nearest warehouse geographically: Name='{Name}', Id={Id}", nearestWarehouse.Name, nearestWarehouse.Id);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "OrderCreatedEventHandler: Failed to determine nearest warehouse for Order {OrderId}", domainEvent.OrderId);
        }
        
        if (nearestWarehouseId == Guid.Empty)
        {
            logger.LogInformation("OrderCreatedEventHandler: nearestWarehouseId is empty. Falling back to first active warehouse.");
            // Fallback
            var allWarehouses = await warehouseQueryService.GetAllAsync(cancellationToken);
            var firstActive = allWarehouses.FirstOrDefault(w => w.IsActive);
            if (firstActive != null)
            {
                nearestWarehouseId = firstActive.Id;
                logger.LogInformation("OrderCreatedEventHandler: Fallback selected active warehouse: Name='{Name}', Id={Id}", firstActive.Name, firstActive.Id);
            }
            else
            {
                logger.LogWarning("OrderCreatedEventHandler: No active warehouse found in database for fallback.");
            }
        }
        else
        {
            logger.LogInformation("OrderCreatedEventHandler: Selected nearestWarehouseId={WarehouseId}", nearestWarehouseId);
        }

        var pickingNoteId = Guid.NewGuid();
        var lineItems = domainEvent.Order.Items.Select(x => new OrderLineItemDto(
            x.ProductId.Value,
            x.ProductCode.Value,
            x.ProductName,
            x.Unit,
            x.Quantity.Value,
            x.Manufacturer
        )).ToList();

        logger.LogInformation("OrderCreatedEventHandler: Publishing OrderCreatedEvent for OrderCode={OrderCode}, NearestWarehouseId={WarehouseId}, LineItemsCount={Count}", 
            domainEvent.OrderCode, nearestWarehouseId, lineItems.Count);

        await publishEndpoint.Publish(new OrderCreatedEvent
        {
            OrderId = domainEvent.OrderId,
            NearestWarehouseId = nearestWarehouseId,
            PickingNoteId = pickingNoteId,
            ActionType = PickingAction.DirectPick, // Picking note processing at warehouse will request supply if needed
            OrderCode = domainEvent.OrderCode,
            CreatedAt = DateTimeOffset.UtcNow,
            ReceiverName = domainEvent.RecipientName,
            ReceiverPhone = domainEvent.RecipientPhone,
            DeliveryAddress = domainEvent.Address,
            CompanyName = domainEvent.CompanyName,
            TaxCode = domainEvent.TaxCode,
            LineItems = lineItems
        }, cancellationToken);

        logger.LogInformation("OrderCreatedEventHandler: [END] OrderCreatedEvent successfully published.");
    }
}