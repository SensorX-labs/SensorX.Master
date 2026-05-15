using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Application.DTOs;
using SensorX.Master.Application.Queries.Warehouses;
using SensorX.Master.Application.Services;

namespace SensorX.Master.Application.Queries.Warehouses.Handlers;

public class GetAllWarehousesQueryHandler(
    IWarehouseQueryService warehouseQueryService // Dùng Dapper Query Service
) : IRequestHandler<GetAllWarehousesQuery, Result<List<WarehouseDto>>>
{
    public async Task<Result<List<WarehouseDto>>> Handle(GetAllWarehousesQuery request, CancellationToken cancellationToken)
    {
        var warehouses = await warehouseQueryService.GetAllAsync(cancellationToken);
        return Result<List<WarehouseDto>>.Success(warehouses, "Warehouses retrieved successfully.");
    }
}