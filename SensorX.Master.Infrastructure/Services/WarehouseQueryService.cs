using Microsoft.EntityFrameworkCore;
using SensorX.Master.Application.DTOs;
using SensorX.Master.Application.Services;
using SensorX.Master.Infrastructure.Persistences;

namespace SensorX.Master.Infrastructure.Services;

public class WarehouseQueryService : IWarehouseQueryService
{
    private readonly AppDbContext _dbContext;

    public WarehouseQueryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<WarehouseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Warehouses
            .AsNoTracking()
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new WarehouseDto(
                w.Id.Value,
                w.Name,
                w.Address,
                w.IsActive,
                w.CreatedAt,
                w.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<WarehouseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Warehouses
            .AsNoTracking()
            .Where(w => w.Id.Value == id)
            .Select(w => new WarehouseDto(
                w.Id.Value,
                w.Name,
                w.Address,
                w.IsActive,
                w.CreatedAt,
                w.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<WarehouseInventoryRowDto>> GetTotalInventoryRowsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.WarehouseInventoryProjections
            .AsNoTracking()
            .OrderByDescending(p => p.LastSyncAt)
            .Select(p => new WarehouseInventoryRowDto(
                p.WarehouseId,
                p.ProductId,
                p.ProductCode,
                p.ProductName,
                p.Unit,
                p.PhysicalQuantity,
                p.AllocatedQuantity,
                p.WarehouseName,
                p.BrandZone,
                p.RackCode,
                p.LastSyncAt))
            .ToListAsync(cancellationToken);
    }
}