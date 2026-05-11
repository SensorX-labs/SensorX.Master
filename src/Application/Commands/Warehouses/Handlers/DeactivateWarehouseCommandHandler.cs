using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Application.Commands.Warehouses;
using SensorX.Master.Domain.Repositories;

namespace SensorX.Master.Application.Commands.Warehouses.Handlers;

public class DeactivateWarehouseCommandHandler(
    IWarehouseRepository warehouseRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeactivateWarehouseCommand, Result>
{
    public async Task<Result> Handle(DeactivateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await warehouseRepository.GetByIdAsync(request.Id, cancellationToken);

        if (warehouse == null)
        {
            return Result.Failure("Warehouse not found.");
        }

        warehouse.Deactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Warehouse deactivated successfully.");
    }
}
