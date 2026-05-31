using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SensorX.Master.Domain.Contexts.SupplyChainContext.ReadModels;
using SensorX.Master.Infrastructure.Persistences;
using SensorX.Warehouse.Application.Events;

namespace SensorX.Master.Application.Events.IntegrationEvents.WarehouseInventory;

public class InventorySnapshotEventConsumer : IConsumer<InventorySnapshotEvent>
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<InventorySnapshotEventConsumer> _logger;

    public InventorySnapshotEventConsumer(AppDbContext dbContext, ILogger<InventorySnapshotEventConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<InventorySnapshotEvent> context)
    {
        if (!Guid.TryParse(context.Message.WarehouseId, out var warehouseId))
        {
            _logger.LogWarning("Invalid WarehouseId received in inventory snapshot: {WarehouseId}", context.Message.WarehouseId);
            return;
        }

        var incomingProductIds = context.Message.Items.Select(x => x.ProductId).ToList();

        // NOTE: Do NOT remove items not in incoming snapshot!
        // InventorySnapshotEvent may be partial (e.g., only contains items modified during stock operations)
        // Removing items would delete existing inventory projections unintentionally.
        // Only update/insert items present in the snapshot. Items not mentioned should remain unchanged.

        foreach (var item in context.Message.Items)
        {
            var entity = await _dbContext.WarehouseInventoryProjections
                .FirstOrDefaultAsync(x => x.WarehouseId == warehouseId && x.ProductId == item.ProductId, context.CancellationToken);

            if (entity is null)
            {
                entity = new WarehouseInventoryProjection
                {
                    WarehouseId = warehouseId,
                    ProductId = item.ProductId
                };
                _dbContext.WarehouseInventoryProjections.Add(entity);
                _logger.LogInformation("Created new WarehouseInventoryProjection for ProductId {ProductId} in warehouse {WarehouseId}", item.ProductId, warehouseId);
            }

            // Only update product details if they are not null to preserve existing data
            if (!string.IsNullOrWhiteSpace(item.ProductCode))
                entity.ProductCode = item.ProductCode;
            if (!string.IsNullOrWhiteSpace(item.ProductName))
                entity.ProductName = item.ProductName;
            if (!string.IsNullOrWhiteSpace(item.Unit))
                entity.Unit = item.Unit;
                
            entity.PhysicalQuantity = item.PhysicalQuantity;
            entity.AllocatedQuantity = item.AllocatedQuantity;
            entity.WarehouseName = item.WarehouseName;
            entity.BrandZone = item.BrandZone;
            entity.RackCode = item.RackCode;
            entity.LastSyncAt = context.Message.Ts;
            
            if (string.IsNullOrWhiteSpace(entity.ProductCode) || string.IsNullOrWhiteSpace(entity.ProductName))
            {
                _logger.LogWarning("InventorySnapshot for ProductId {ProductId} from warehouse {WarehouseId} has missing product details (Code: {Code}, Name: {Name}). This may indicate product sync issues.", 
                    item.ProductId, warehouseId, item.ProductCode ?? "null", item.ProductName ?? "null");
            }
        }

        await _dbContext.SaveChangesAsync(context.CancellationToken);
        _logger.LogInformation("Synced {Count} inventory items for warehouse {WarehouseId}", context.Message.Items.Count, warehouseId);
    }
}
