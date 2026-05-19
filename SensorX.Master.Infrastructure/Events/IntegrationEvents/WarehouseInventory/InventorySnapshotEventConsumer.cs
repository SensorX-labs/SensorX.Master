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

        var obsoleteProjections = await _dbContext.WarehouseInventoryProjections
            .Where(x => x.WarehouseId == warehouseId && !incomingProductIds.Contains(x.ProductId))
            .ToListAsync(context.CancellationToken);

        if (obsoleteProjections.Any())
        {
            _dbContext.WarehouseInventoryProjections.RemoveRange(obsoleteProjections);
            _logger.LogInformation("Removed {Count} obsolete inventory projections for warehouse {WarehouseId}", obsoleteProjections.Count, warehouseId);
        }

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
            }

            entity.ProductCode = item.ProductCode;
            entity.ProductName = item.ProductName;
            entity.Unit = item.Unit;
            entity.PhysicalQuantity = item.PhysicalQuantity;
            entity.AllocatedQuantity = item.AllocatedQuantity;
            entity.WarehouseName = item.WarehouseName;
            entity.BrandZone = item.BrandZone;
            entity.RackCode = item.RackCode;
            entity.LastSyncAt = context.Message.Ts;
        }

        await _dbContext.SaveChangesAsync(context.CancellationToken);
        _logger.LogInformation("Synced {Count} inventory items for warehouse {WarehouseId}", context.Message.Items.Count, warehouseId);
    }
}
