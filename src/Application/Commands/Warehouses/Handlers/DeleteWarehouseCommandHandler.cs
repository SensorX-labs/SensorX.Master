using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Application.Commands.Warehouses;
using SensorX.Master.Domain.Repositories;

namespace SensorX.Master.Application.Commands.Warehouses.Handlers;

public class DeleteWarehouseCommandHandler(
    IWarehouseRepository warehouseRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteWarehouseCommand, Result>
{
    public async Task<Result> Handle(DeleteWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await warehouseRepository.GetByIdAsync(request.Id, cancellationToken);

        if (warehouse == null)
        {
            return Result.Failure("Warehouse not found.");
        }

        await warehouseRepository.DeleteAsync(warehouse, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Warehouse deleted successfully.");
    }
}
