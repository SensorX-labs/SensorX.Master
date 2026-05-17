using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.WarehouseAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Infrastructure.Persistences;

namespace SensorX.Master.Infrastructure.Repositories;

public class WarehouseRepository : Repository<SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.WarehouseAggregate.Warehouse>, IWarehouseRepository
{
    private readonly AppDbContext _appDbContext;

    public WarehouseRepository(AppDbContext dbContext) : base(dbContext)
    {
        _appDbContext = dbContext;
    }

    public async Task<SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.WarehouseAggregate.Warehouse?> GetByIdAsync(WarehouseId id, CancellationToken cancellationToken = default)
    {
        return await _appDbContext.Warehouses
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }
}