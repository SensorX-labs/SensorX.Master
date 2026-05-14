using SensorX.Master.Domain.AggregatesModel;
using SensorX.Master.Domain.Common;

namespace SensorX.Master.Domain.Repositories;

public interface IWarehouseRepository : IRepository<Warehouse>
{
    Task<Warehouse?> GetByApiEndpointUrlAsync(string apiEndpointUrl, CancellationToken cancellationToken = default);
    Task<List<Warehouse>> GetActiveWarehousesAsync(CancellationToken cancellationToken = default);
}
