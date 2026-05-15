using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.SupplyRequestAggregate;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.WarehouseAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.UseCases.SupplyRequests.Commands.CreateSupplyRequest;

public class CreateSupplyRequestCommandHandler(
    IRepository<SupplyRequest> supplyRequestRepository,
    IRepository<Warehouse> warehouseRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateSupplyRequestCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateSupplyRequestCommand request, CancellationToken cancellationToken)
    {
        var warehouseId = new WarehouseId(request.WarehouseId);
        var warehouse = await warehouseRepository.GetByIdAsync(warehouseId, cancellationToken);
        if (warehouse == null || !warehouse.IsActive)
        {
            return Result<Guid>.Failure("Kho yêu cầu không tồn tại hoặc đã bị vô hiệu hóa");
        }

        var code = Code.From(string.IsNullOrWhiteSpace(request.Code) ? $"YC_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}" : request.Code);
        var supplyRequest = new SupplyRequest(
            SupplyRequestId.New(),
            code,
            warehouseId,
            SupplyRequestStatus.Pending,
            request.Note ?? ""
        );

        if (request.Items != null)
        {
            foreach (var item in request.Items)
            {
                if (item.RequestedQuantity > 0)
                {
                    supplyRequest.AddItem(new ProductId(item.ProductId), new Quantity(item.RequestedQuantity));
                }
            }
        }

        await supplyRequestRepository.Add(supplyRequest, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(supplyRequest.Id.Value);
    }
}
