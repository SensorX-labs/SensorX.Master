using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Services;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Infrastructure.Persistences;
using SensorX.Warehouse.Application.Events;
using WarehouseEntity = SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.WarehouseAggregate.Warehouse;

namespace SensorX.Master.Application.Events.IntegrationEvents.WarehouseInventory;

public class WarehouseConnectedEventConsumer : IConsumer<WarehouseConnectedEvent>
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<WarehouseConnectedEventConsumer> _logger;
    private readonly IGeolocationQueryService _geolocationQueryService;

    public WarehouseConnectedEventConsumer(AppDbContext dbContext, ILogger<WarehouseConnectedEventConsumer> logger, IGeolocationQueryService geolocationQueryService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _geolocationQueryService = geolocationQueryService;
    }

    public async Task Consume(ConsumeContext<WarehouseConnectedEvent> context)
    {
        var message = context.Message;
        if (!Guid.TryParse(message.WarehouseId, out var idGuid))
        {
            _logger.LogWarning("Invalid WarehouseId format in WarehouseConnectedEvent: {WarehouseId}", message.WarehouseId);
            return;
        }

        var name = string.IsNullOrWhiteSpace(message.WarehouseName)
            ? $"Warehouse {message.WarehouseId[..Math.Min(message.WarehouseId.Length, 8)]}"
            : message.WarehouseName.Trim();

        var geolocationCandidates = await _geolocationQueryService.GetGeolocationByAddress(message.WarehouseAddress, context.CancellationToken);
        var geolocation = geolocationCandidates?.FirstOrDefault() ?? new(0, 0);

        var createdAt = DateTimeOffset.UtcNow;
        var updatedAt = DateTimeOffset.UtcNow;
        var rowsAffected = await _dbContext.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO ""Warehouses"" (
                ""Id"", ""Address"", ""CreatedAt"", ""IsActive"", ""Name"", ""UpdatedAt"", ""Location_Latitude"", ""Location_Longitude""
            )
            VALUES (
                {idGuid}, {message.WarehouseAddress}, {createdAt}, {true}, {name}, {null}, {geolocation.Latitude}, {geolocation.Longitude}
            )
            ON CONFLICT (""Id"") DO UPDATE SET
                ""Address"" = EXCLUDED.""Address"",
                ""IsActive"" = EXCLUDED.""IsActive"",
                ""Name"" = EXCLUDED.""Name"",
                ""UpdatedAt"" = {updatedAt},
                ""Location_Latitude"" = EXCLUDED.""Location_Latitude"",
                ""Location_Longitude"" = EXCLUDED.""Location_Longitude"";
        ", context.CancellationToken);

        if (rowsAffected > 0)
        {
            if (rowsAffected == 1)
            {
                _logger.LogInformation("Successfully upserted warehouse: {WarehouseName} ({WarehouseId})", name, message.WarehouseId);
                return;
            }

            _logger.LogInformation("Warehouse updated from WarehouseConnected event: {WarehouseName} ({WarehouseId})", name, message.WarehouseId);
            return;
        }

        _logger.LogInformation("Warehouse upsert had no effect: {WarehouseId}", message.WarehouseId);
    }
}
