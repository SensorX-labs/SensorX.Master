using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SensorX.Master.Domain.AggregatesModel;
using SensorX.Master.Domain.Repositories;
using SensorX.Master.Infrastructure.Persistence;

namespace SensorX.Master.Infrastructure.Repositories;

public class WarehouseRepository : EfRepository<Warehouse>, IWarehouseRepository
{
    public WarehouseRepository(AppDbContext dbContext) : base(dbContext) { }

    public async Task<Warehouse?> GetByApiEndpointUrlAsync(string apiEndpointUrl, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Warehouses
            .FirstOrDefaultAsync(w => w.ApiEndpointUrl.Value == apiEndpointUrl, cancellationToken);
    }

    public async Task<List<Warehouse>> GetActiveWarehousesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Warehouses
            .Where(w => w.IsActive)
            .ToListAsync(cancellationToken);
    }
}
