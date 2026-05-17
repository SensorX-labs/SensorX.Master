using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.WarehouseAggregate;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.Commands.Warehouses.CreateWarehouse;

public class CreateWarehouseCommandHandler(
    IWarehouseRepository warehouseRepository
) : IRequestHandler<CreateWarehouseCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Warehouse.Name))
        {
            return Result<Guid>.Failure("Tên kho không được để trống");
        }

        var warehouseId = request.Warehouse.Id == Guid.Empty
            ? WarehouseId.New()
            : new WarehouseId(request.Warehouse.Id);

        var warehouse = new SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.WarehouseAggregate.Warehouse(
            warehouseId,
            request.Warehouse.Name.Trim(),
            string.IsNullOrWhiteSpace(request.Warehouse.Address) ? null : request.Warehouse.Address.Trim()
        );

        await warehouseRepository.AddAsync(warehouse, cancellationToken);

        return Result<Guid>.Success(warehouse.Id.Value);
    }
}