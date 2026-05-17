using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Infrastructure.Persistences;
using SensorX.Warehouse.Application.Events;
using WarehouseEntity = SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.WarehouseAggregate.Warehouse;

namespace SensorX.Master.Application.Events.IntegrationEvents.WarehouseInventory;

public class WarehouseConnectedEventConsumer : IConsumer<WarehouseConnectedEvent>
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<WarehouseConnectedEventConsumer> _logger;

    public WarehouseConnectedEventConsumer(AppDbContext dbContext, ILogger<WarehouseConnectedEventConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<WarehouseConnectedEvent> context)
    {
        var message = context.Message;
        if (!Guid.TryParse(message.WarehouseId, out var idGuid))
        {
            _logger.LogWarning("Invalid WarehouseId format in WarehouseConnectedEvent: {WarehouseId}", message.WarehouseId);
            return;
        }

        var warehouseId = new WarehouseId(idGuid);
        var warehouseExists = await _dbContext.Warehouses.AnyAsync(w => w.Id == warehouseId, context.CancellationToken);

        if (!warehouseExists)
        {
            var name = string.IsNullOrWhiteSpace(message.WarehouseName)
                ? $"Warehouse {message.WarehouseId[..Math.Min(message.WarehouseId.Length, 8)]}"
                : message.WarehouseName.Trim();

            var warehouse = new WarehouseEntity(
                warehouseId,
                name,
                "Auto-registered on connection via RabbitMQ"
            );

            _dbContext.Warehouses.Add(warehouse);
            await _dbContext.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation("Successfully auto-registered new warehouse: {WarehouseName} ({WarehouseId})", name, message.WarehouseId);
        }
        else
        {
            _logger.LogInformation("Warehouse already exists, skipped registration: {WarehouseId}", message.WarehouseId);
        }
    }
}
