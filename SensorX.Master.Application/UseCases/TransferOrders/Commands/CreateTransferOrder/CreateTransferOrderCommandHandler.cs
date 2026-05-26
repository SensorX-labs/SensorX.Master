using System.Net.Http;
using System.Text.Json;
using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.TransferOrderAggregate;
using SensorX.Master.Domain.Events;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.UseCases.TransferOrders.Commands.CreateTransferOrder;

public class CreateTransferOrderCommandHandler(
    IRepository<TransferOrder> transferOrderRepository,
    IRepository<SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.WarehouseAggregate.Warehouse> warehouseRepository,
    IMediator mediator
) : IRequestHandler<CreateTransferOrderCommand, Result<Guid>>
{
    private record InventoryItemStockDto(
        Guid ProductId,
        decimal PhysicalQuantity,
        decimal AllocatedQuantity
    );

    public async Task<Result<Guid>> Handle(CreateTransferOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            return Result<Guid>.Failure("Danh sách sản phẩm điều chuyển không được để trống");
        }

        var sourceWarehouseId = new WarehouseId(request.SourceWarehouseId);
        var sourceWarehouse = await warehouseRepository.GetByIdAsync(sourceWarehouseId, cancellationToken);
        if (sourceWarehouse is null || !sourceWarehouse.IsActive)
        {
            return Result<Guid>.Failure("Kho xuất không tồn tại hoặc đã bị vô hiệu hóa");
        }

        var code = Code.From(request.Code);
        var destinationWarehouseId = new WarehouseId(request.DestinationWarehouseId);

        var transferOrder = new TransferOrder(
            new TransferOrderId(Guid.NewGuid()),
            code,
            sourceWarehouseId,
            destinationWarehouseId,
            TransferOrderStatus.Processing,
            request.Note,
            null
        );

        foreach (var itemDto in request.Items)
        {
            transferOrder.AddItem(
                new ProductId(itemDto.ProductId),
                Code.From(itemDto.ProductCode),
                itemDto.ProductName,
                itemDto.Unit,
                new Quantity(itemDto.Quantity),
                itemDto.ManufactureName,
                itemDto.Note ?? ""
            );
        }

        await transferOrderRepository.AddAsync(transferOrder, cancellationToken);

        // Domain events are automatically published by the Unit of Work when saving changes

        return Result<Guid>.Success(transferOrder.Id.Value);
    }
}
