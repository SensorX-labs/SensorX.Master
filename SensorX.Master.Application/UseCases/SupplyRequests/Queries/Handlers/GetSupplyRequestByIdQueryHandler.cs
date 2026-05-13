using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.SupplyRequestAggregate;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.TransferOrderAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Application.UseCases.SupplyRequests.Queries.Handlers;

public class GetSupplyRequestByIdQueryHandler(
    IRepository<SupplyRequest> supplyRequestRepository,
    IRepository<TransferOrder> transferOrderRepository
) : IRequestHandler<GetSupplyRequestByIdQuery, Result<SupplyRequestDetailDto>>
{
    public async Task<Result<SupplyRequestDetailDto>> Handle(GetSupplyRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var targetId = new SupplyRequestId(request.Id);
        var allRequests = await supplyRequestRepository.ListAsync(cancellationToken);
        var supplyRequest = allRequests.FirstOrDefault(x => x.Id == targetId);

        if (supplyRequest == null)
        {
            return Result<SupplyRequestDetailDto>.Failure("Yêu cầu cung ứng không tồn tại");
        }

        var allTransferOrders = await transferOrderRepository.ListAsync(cancellationToken);
        var transferOrders = allTransferOrders.Where(to => to.SupplyRequestId == targetId).ToList();

        var itemsDto = supplyRequest.Items.Select(i => new SupplyRequestItemDetailDto(
            i.Id.Value,
            i.ProductId.Value,
            i.RequestedQuantity.Value
        )).ToList();

        var purchaseOptionsDto = supplyRequest.PurchaseOptions.Select(p => new PurchaseOptionDetailDto(
            p.Id.Value,
            p.ProductId.Value,
            p.Quantity.Value,
            p.Note ?? ""
        )).ToList();

        var transferOrdersDto = transferOrders.Select(to => new TransferPlanDetailDto(
            to.Id.Value,
            to.Code.Value,
            to.SourceWarehouseId.Value,
            to.DestinationWarehouseId.Value,
            to.Status.ToString(),
            to.Note ?? "",
            to.Items.Select(ti => new TransferPlanItemDetailDto(
                ti.Id.Value,
                ti.ProductId.Value,
                ti.ProductCode.Value,
                ti.ProductName,
                ti.Quantity.Value,
                ti.Unit,
                ti.ManufactureName ?? "",
                ti.Note ?? ""
            )).ToList()
        )).ToList();

        var detailDto = new SupplyRequestDetailDto(
            supplyRequest.Id.Value,
            supplyRequest.Code.Value,
            supplyRequest.WarehouseId.Value,
            supplyRequest.Status.ToString(),
            supplyRequest.Note ?? "",
            supplyRequest.CreatedAt,
            itemsDto,
            purchaseOptionsDto,
            transferOrdersDto
        );

        return Result<SupplyRequestDetailDto>.Success(detailDto);
    }
}
