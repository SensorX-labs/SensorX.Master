using SensorX.Master.Application.DTOs;

namespace SensorX.Master.Application.Services;

public interface IWarehouseQueryService
{
    Task<List<WarehouseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<WarehouseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}