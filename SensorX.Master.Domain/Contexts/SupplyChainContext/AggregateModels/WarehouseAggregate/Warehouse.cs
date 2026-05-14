using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.WarehouseAggregate;

public class Warehouse : Entity<WarehouseId>, IAggregateRoot, ICreationTrackable
{
    public Warehouse(WarehouseId id, string name, string? address, ApiEndpointUrl apiEndpointUrl, bool isActive = true) : base(id)
    {
        Name = name;
        Address = address;
        ApiEndpointUrl = apiEndpointUrl;
        IsActive = isActive;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string Name { get; private set; }
    public string? Address { get; private set; }
    public ApiEndpointUrl ApiEndpointUrl { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }

    public void Update(string name, string? address, ApiEndpointUrl apiEndpointUrl, bool isActive)
    {
        Name = name;
        Address = address;
        ApiEndpointUrl = apiEndpointUrl;
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
