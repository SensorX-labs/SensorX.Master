using System.Net.Http;
using System.Text.Json;
using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.TransferOrderAggregate;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.WarehouseAggregate;
using SensorX.Master.Domain.Events;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.UseCases.TransferOrders.Commands.CreateTransferOrder;

public class CreateTransferOrderCommandHandler(
    IRepository<TransferOrder> transferOrderRepository,
    IRepository<Warehouse> warehouseRepository,
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

        // Thực hiện kiểm tra tồn kho chéo qua HTTP tới service quản lý kho xuất
        try
        {
            using var client = new HttpClient();
            var baseUrl = sourceWarehouse.ApiEndpointUrl.Value.TrimEnd('/');
            var url = $"{baseUrl}/api/inventory/list?pageSize=1000";

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
            httpRequest.Headers.Add("X-Warehouse-Id", sourceWarehouse.Id.ToString());

            var response = await client.SendAsync(httpRequest, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var doc = await JsonDocument.ParseAsync(contentStream, cancellationToken: cancellationToken);

                var stockItems = new List<InventoryItemStockDto>();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                if (doc.RootElement.TryGetProperty("items", out var itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in itemsElement.EnumerateArray())
                    {
                        var dto = JsonSerializer.Deserialize<InventoryItemStockDto>(item.GetRawText(), options);
                        if (dto != null)
                        {
                            stockItems.Add(dto);
                        }
                    }
                }

                // So sánh số lượng khả dụng của từng mặt hàng
                foreach (var itemReq in request.Items)
                {
                    var stock = stockItems.FirstOrDefault(x => x.ProductId == itemReq.ProductId);
                    var salable = stock != null ? stock.PhysicalQuantity - stock.AllocatedQuantity : 0;
                    if (itemReq.Quantity > salable)
                    {
                        return Result<Guid>.Failure($"Kho xuất không đủ hàng cho sản phẩm {itemReq.ProductName}. Khả dụng: {salable}, Yêu cầu: {itemReq.Quantity}");
                    }
                }
            }
            else
            {
                return Result<Guid>.Failure($"Không thể xác thực tồn kho từ kho xuất. Phản hồi HTTP: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Lỗi kết nối khi xác thực tồn kho kho xuất: {ex.Message}");
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

        // Publish domain event
        await mediator.Publish(new TransferOrderCreatedDomainEvent(
            transferOrder.Id.Value,
            code.Value,
            sourceWarehouseId.Value
        ), cancellationToken);

        return Result<Guid>.Success(transferOrder.Id.Value);
    }
}
