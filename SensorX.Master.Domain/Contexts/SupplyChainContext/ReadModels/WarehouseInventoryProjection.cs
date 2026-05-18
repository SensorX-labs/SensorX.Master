namespace SensorX.Master.Domain.Contexts.SupplyChainContext.ReadModels;

public class WarehouseInventoryProjection
{
    public Guid WarehouseId { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public string? Unit { get; set; }
    public int PhysicalQuantity { get; set; }
    public int AllocatedQuantity { get; set; }
    public string? WarehouseName { get; set; }
    public string? BrandZone { get; set; }
    public string? RackCode { get; set; }
    public DateTimeOffset LastSyncAt { get; set; }
}
