using System.Threading;
using System.Threading.Tasks;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.WarehouseAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.WarehouseAggregate;

public interface IWarehouseRepository : IRepository<Warehouse>
{
    Task<Warehouse?> GetByIdAsync(WarehouseId id, CancellationToken cancellationToken = default);
}
