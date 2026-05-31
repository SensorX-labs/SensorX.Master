using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.UseCases.TransferOrders.Queries;

public record TransferOrderItemDetailDto(
    Guid Id,
    Guid ProductId,
    string ProductCode,
    string ProductName,
    string Unit,
    int Quantity,
    string ManufacturerName,
    string Note
);

public record TransferOrderDetailDto(
    Guid Id,
    string Code,
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    string Status,
    string Note,
    List<TransferOrderItemDetailDto> Items,
    Guid? SupplyRequestId = null
);

public record TransferOrderListItemDto(
    Guid Id,
    string Code,
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    string Status,
    string Note,
    DateTimeOffset CreatedAt
);

public record GetTransferOrderByIdQuery(Guid Id) : IRequest<Result<TransferOrderDetailDto>>;
public record GetPageListTransferOrdersQuery(int Page = 1, int PageSize = 20) : IRequest<Result<object>>;
