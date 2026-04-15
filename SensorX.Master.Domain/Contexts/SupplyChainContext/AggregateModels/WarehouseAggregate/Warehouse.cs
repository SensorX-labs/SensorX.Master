using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.WarehouseAggregate;

public class Warehouse : Entity<WarehouseId>, IAggregateRoot, ICreationTrackable
{
    public Warehouse(WarehouseId id, string name) : base(id)
    {
        Name = name;
    }

    public string Name { get; private set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
