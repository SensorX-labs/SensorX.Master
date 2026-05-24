using MassTransit;

namespace SensorX.Warehouse.Application.Events;

public record InventoryItemSnapshot(
    Guid ProductId,
    string? ProductCode,
    string? ProductName,
    string? Unit,
    int PhysicalQuantity,
    int AllocatedQuantity,
    string? WarehouseName,
    string? BrandZone,
    string? RackCode
);

[EntityName("Inventory-Snapshot-Event")]
public record InventorySnapshotEvent(
    string WarehouseId,
    DateTimeOffset Ts,
    IReadOnlyList<InventoryItemSnapshot> Items
);

[EntityName("Warehouse-Connected-Event")]
public record WarehouseConnectedEvent(
    string WarehouseId,
    string WarehouseName,
    string WarehouseAddress,
    string Status,
    DateTimeOffset Ts
);
