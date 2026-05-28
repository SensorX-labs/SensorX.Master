using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.TransferOrderAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Application.UseCases.TransferOrders.Queries.Handlers;

public class GetTransferOrderByIdQueryHandler(
    IRepository<TransferOrder> transferOrderRepository
) : IRequestHandler<GetTransferOrderByIdQuery, Result<TransferOrderDetailDto>>
{
    public async Task<Result<TransferOrderDetailDto>> Handle(GetTransferOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var targetId = new TransferOrderId(request.Id);
        var allTransferOrders = await transferOrderRepository.ListAsync(cancellationToken);
        var transferOrder = allTransferOrders.FirstOrDefault(x => x.Id == targetId);

        if (transferOrder == null)
        {
            return Result<TransferOrderDetailDto>.Failure("Lệnh điều chuyển không tồn tại");
        }

        var itemsDto = transferOrder.Items.Select(ti => new TransferOrderItemDetailDto(
            ti.Id.Value,
            ti.ProductId.Value,
            ti.ProductCode.Value,
            ti.ProductName,
            ti.Unit,
            ti.Quantity.Value,
            ti.ManufactureName ?? "",
            ti.Note ?? ""
        )).ToList();

        var detailDto = new TransferOrderDetailDto(
            transferOrder.Id.Value,
            transferOrder.Code.Value,
            transferOrder.SourceWarehouseId.Value,
            transferOrder.DestinationWarehouseId.Value,
            transferOrder.Status.ToString(),
            transferOrder.Note ?? "",
            itemsDto,
            transferOrder.SupplyRequestId?.Value
        );

        return Result<TransferOrderDetailDto>.Success(detailDto);
    }
}
