using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Application.DTOs;
using SensorX.Master.Application.Queries.Warehouses;
using SensorX.Master.Application.Services;

namespace SensorX.Master.Application.Queries.Warehouses.Handlers;

public class GetWarehouseByIdQueryHandler(
    IWarehouseQueryService warehouseQueryService
) : IRequestHandler<GetWarehouseByIdQuery, Result<WarehouseDto>>
{
    public async Task<Result<WarehouseDto>> Handle(GetWarehouseByIdQuery request, CancellationToken cancellationToken)
    {
        var warehouse = await warehouseQueryService.GetByIdAsync(request.Id, cancellationToken);
        
        if (warehouse == null)
            return Result<WarehouseDto>.Failure("Warehouse not found.");
        
        return Result<WarehouseDto>.Success(warehouse, "Warehouse retrieved successfully.");
    }
}