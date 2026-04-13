using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.WarehouseAggregate;

public class Warehouse : Entity<WarehouseId>
{
    public string Name { get; private set; }
}
