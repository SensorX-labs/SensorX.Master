using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.WarehouseAggregate;

public class Warehouse(WarehouseId id, string name, string? address, Geolocation location, bool isActive = true)
 : Entity<WarehouseId>(id), IAggregateRoot, ICreationTrackable
{
    public string Name { get; private set; } = name;
    public string? Address { get; private set; } = address;
    public bool IsActive { get; private set; } = isActive;
    public Geolocation Location { get; set; } = location;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }

    // For EF Core materialization
    private Warehouse() : this(null!, null!, null, null!, false) { }

    public void Update(string name, string? address, Geolocation location, bool isActive)
    {
        Name = name;
        Address = address;
        Location = location;
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
