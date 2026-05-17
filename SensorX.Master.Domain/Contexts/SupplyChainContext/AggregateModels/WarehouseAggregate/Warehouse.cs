using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.WarehouseAggregate;

public class Warehouse(WarehouseId id, string name, string? address, bool isActive = true)
 : Entity<WarehouseId>(id), IAggregateRoot, ICreationTrackable
{
    public string Name { get; private set; } = name;
    public string? Address { get; private set; } = address;
    public bool IsActive { get; private set; } = isActive;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }

    public void Update(string name, string? address, bool isActive)
    {
        Name = name;
        Address = address;
        IsActive = isActive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
