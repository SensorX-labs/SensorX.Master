using System;
using SensorX.Master.Domain.Common;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.AggregatesModel;

public class Warehouse : Entity<WarehouseId>, IAggregateRoot
{
    public string Name { get; private set; } = null!;
    public string? Address { get; private set; }
    public ApiEndpointUrl ApiEndpointUrl { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    private Warehouse() { }

    public Warehouse(
        WarehouseId id,
        string name,
        string? address,
        ApiEndpointUrl apiEndpointUrl,
        bool isActive = true)
    {
        Id = id;
        Name = name;
        Address = address;
        ApiEndpointUrl = apiEndpointUrl;
        IsActive = isActive;
        CreatedAt = DateTimeOffset.UtcNow;
    }

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
