namespace SensorX.Master.Application.DTOs;

public record WarehouseInventoryRowDto(
    Guid WarehouseId,
    Guid ProductId,
    string? ProductCode,
    string? ProductName,
    string? Unit,
    int PhysicalQuantity,
    int AllocatedQuantity,
    string? WarehouseName,
    string? BrandZone,
    string? RackCode,
    DateTimeOffset LastSyncAt
)
{
    public WarehouseInventoryRowDto() : this(Guid.Empty, Guid.Empty, null, null, null, 0, 0, null, null, null, default) { }
}
