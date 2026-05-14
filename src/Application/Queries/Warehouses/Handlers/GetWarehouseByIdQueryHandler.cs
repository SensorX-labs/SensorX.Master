using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Application.DTOs;
using SensorX.Master.Application.Queries.Warehouses;
using SensorX.Master.Domain.Repositories;

namespace SensorX.Master.Application.Queries.Warehouses.Handlers;

public class GetWarehouseByIdQueryHandler(
    IWarehouseRepository warehouseRepository
) : IRequestHandler<GetWarehouseByIdQuery, Result<WarehouseDto>>
{
    public async Task<Result<WarehouseDto>> Handle(GetWarehouseByIdQuery request, CancellationToken cancellationToken)
    {
        var warehouse = await warehouseRepository.GetByIdAsync(request.Id, cancellationToken);

        if (warehouse == null)
        {
            return Result<WarehouseDto>.Failure("Warehouse not found.");
        }

        var dto = new WarehouseDto(
            warehouse.Id.Value,
            warehouse.Name,
            warehouse.Address,
            warehouse.ApiEndpointUrl.Value,
            warehouse.IsActive,
            warehouse.CreatedAt,
            warehouse.UpdatedAt
        );

        return Result<WarehouseDto>.Success(dto, "Warehouse retrieved successfully.");
    }
}
