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
            
        // Calculate NearestWarehouseId
        Guid nearestWarehouseId = Guid.Empty;
        var geolocations = await geolocationQueryService.GetGeolocationByAddress(domainEvent.Address, cancellationToken);
        if (geolocations != null && geolocations.Count > 0 && geolocations.First() != null)
        {
            var geo = geolocations.First()!;
            var nearestWarehouse = await warehouseQueryService.FindNearestWarehouseAsync(geo.Latitude, geo.Longitude, cancellationToken);
            if (nearestWarehouse != null)
            {
                nearestWarehouseId = nearestWarehouse.Id;
            }
        }
        
        if (nearestWarehouseId == Guid.Empty)
        {
            // Fallback
            var allWarehouses = await warehouseQueryService.GetAllAsync(cancellationToken);
            var firstActive = allWarehouses.FirstOrDefault(w => w.IsActive);
            if (firstActive != null)
            {
                nearestWarehouseId = firstActive.Id;
            }
        }

        // Master-Centric Smart Picking Logic
        var pickingNoteId = Guid.NewGuid();
        PickingAction actionType;
        Guid? linkedTransferOrderId = null;
        Guid? linkedSupplyRequestId = null;

        bool nearestSufficient = domainEvent.Order.Items.All(item => 
        {
            var inv = inventoryRows.FirstOrDefault(x => x.WarehouseId == nearestWarehouseId && x.ProductId == item.ProductId.Value);
            return inv != null && (inv.PhysicalQuantity - inv.AllocatedQuantity) >= item.Quantity.Value;
        });

        if (nearestSufficient)
        {
            actionType = PickingAction.DirectPick;
            logger.LogInformation("Order {OrderId} will be Direct Picked from nearest warehouse {WarehouseId}", domainEvent.OrderId, nearestWarehouseId);
        }
        else
        {
            // Find a single other warehouse that can fulfill the ENTIRE order
            var validSourceWarehouses = inventoryRows
                .Where(x => x.WarehouseId != nearestWarehouseId)
                .GroupBy(x => x.WarehouseId)
                .Where(g => domainEvent.Order.Items.All(item => 
                {
                    var inv = g.FirstOrDefault(x => x.ProductId == item.ProductId.Value);
                    return inv != null && (inv.PhysicalQuantity - inv.AllocatedQuantity) >= item.Quantity.Value;
                }))
                .Select(g => g.Key)
                .ToList();

            if (validSourceWarehouses.Any())
            {
                actionType = PickingAction.WaitingTransfer;
                // Pick the first valid one (could be optimized by distance later)
                var sourceWarehouseId = validSourceWarehouses.First();

                var transferOrder = new SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.TransferOrderAggregate.TransferOrder(
                    new SensorX.Master.Domain.StrongIDs.TransferOrderId(Guid.NewGuid()),
                    SensorX.Master.Domain.ValueObjects.Code.Create($"TO-{domainEvent.OrderCode}"),
                    new SensorX.Master.Domain.StrongIDs.WarehouseId(sourceWarehouseId),
                    new SensorX.Master.Domain.StrongIDs.WarehouseId(nearestWarehouseId),
                    SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.TransferOrderAggregate.TransferOrderStatus.Processing,
                    $"Auto generated for Order {domainEvent.OrderCode}",
                    null,
                    pickingNoteId
                );

                foreach (var itemDto in domainEvent.Order.Items)
                {
                    transferOrder.AddItem(
                        new SensorX.Master.Domain.StrongIDs.ProductId(itemDto.ProductId.Value),
                        SensorX.Master.Domain.ValueObjects.Code.From(itemDto.ProductCode.Value),
                        itemDto.ProductName,
                        itemDto.Unit,
                        new SensorX.Master.Domain.ValueObjects.Quantity(itemDto.Quantity.Value),
                        itemDto.Manufacturer,
                        ""
                    );
                }

                await transferOrderRepository.Add(transferOrder, cancellationToken);
                linkedTransferOrderId = transferOrder.Id.Value;
                logger.LogInformation("Order {OrderId} triggers TransferOrder {TransferOrderId} from {Source} to {Dest}", domainEvent.OrderId, transferOrder.Id.Value, sourceWarehouseId, nearestWarehouseId);
            }
            else
            {
                actionType = PickingAction.WaitingSupply;
                var supplyRequest = new SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.SupplyRequestAggregate.SupplyRequest(
                    new SensorX.Master.Domain.StrongIDs.SupplyRequestId(Guid.NewGuid()),
                    SensorX.Master.Domain.ValueObjects.Code.Create($"SR-{domainEvent.OrderCode}"),
                    new SensorX.Master.Domain.StrongIDs.WarehouseId(nearestWarehouseId),
                    SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.SupplyRequestAggregate.SupplyRequestStatus.Pending,
                    $"Auto generated for Order {domainEvent.OrderCode}",
                    pickingNoteId
                );

                foreach (var itemDto in domainEvent.Order.Items)
                {
                    var nearestInv = inventoryRows.FirstOrDefault(x => x.WarehouseId == nearestWarehouseId && x.ProductId == itemDto.ProductId.Value);
                    var nearestAvailable = nearestInv != null ? (nearestInv.PhysicalQuantity - nearestInv.AllocatedQuantity) : 0;
                    
                    if (nearestAvailable < itemDto.Quantity.Value)
                    {
                        supplyRequest.AddItem(
                            new SensorX.Master.Domain.StrongIDs.ProductId(itemDto.ProductId.Value),
                            new SensorX.Master.Domain.ValueObjects.Quantity(itemDto.Quantity.Value - (int)nearestAvailable)
                        );
                    }
                }

                await supplyRequestRepository.Add(supplyRequest, cancellationToken);
                linkedSupplyRequestId = supplyRequest.Id.Value;
                logger.LogInformation("Order {OrderId} triggers SupplyRequest {SupplyRequestId} for nearest warehouse {Dest}", domainEvent.OrderId, supplyRequest.Id.Value, nearestWarehouseId);
            }
        }

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

        if (assignedWarehouseId == null)
        {
            var warehouses = await warehouseQueryService.GetAllAsync(cancellationToken);
            assignedWarehouseId = warehouses.FirstOrDefault()?.Id;
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

        await publishEndpoint.Publish(new OrderCreatedEvent
        {
            OrderId = domainEvent.OrderId,
            NearestWarehouseId = nearestWarehouseId,
            PickingNoteId = pickingNoteId,
            ActionType = actionType,
            OrderCode = domainEvent.OrderCode,
            PickingNoteId = pickingNoteId,
            CreatedAt = DateTimeOffset.UtcNow,
            ReceiverName = domainEvent.RecipientName,
            ReceiverPhone = domainEvent.RecipientPhone,
            DeliveryAddress = domainEvent.Address,
            CompanyName = domainEvent.CompanyName,
            TaxCode = domainEvent.TaxCode,
            NearestWarehouseId = assignedWarehouseId ?? Guid.Empty,
            LineItems = lineItems
        }, cancellationToken);
    }
}