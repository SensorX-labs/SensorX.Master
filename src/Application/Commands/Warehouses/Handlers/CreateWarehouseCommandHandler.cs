using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Application.Commands.Warehouses;
using SensorX.Master.Application.DTOs;
using SensorX.Master.Domain.AggregatesModel;
using SensorX.Master.Domain.Repositories;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.Commands.Warehouses.Handlers;

public class CreateWarehouseCommandHandler(
    IWarehouseRepository warehouseRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateWarehouseCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var existingWarehouse = await warehouseRepository.GetByApiEndpointUrlAsync(
            request.Warehouse.ApiEndpointUrl,
            cancellationToken
        );

        if (existingWarehouse != null)
        {
            return Result<Guid>.Failure("Warehouse with this API endpoint already exists.");
        }

        var warehouse = new Warehouse(
            WarehouseId.New(),
            request.Warehouse.Name,
            request.Warehouse.Address,
            ApiEndpointUrl.From(request.Warehouse.ApiEndpointUrl),
            true
        );

        await warehouseRepository.AddAsync(warehouse, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(warehouse.Id.Value, "Warehouse created successfully.");
    }
}
