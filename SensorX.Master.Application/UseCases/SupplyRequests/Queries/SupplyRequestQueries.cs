using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.UseCases.SupplyRequests.Queries;

public record SupplyRequestItemDetailDto(
    Guid Id,
    Guid ProductId,
    int RequestedQuantity
);

public record PurchaseOptionDetailDto(
    Guid Id,
    Guid ProductId,
    int Quantity,
    string Note
);

public record TransferPlanItemDetailDto(
    Guid Id,
    Guid ProductId,
    string ProductCode,
    string ProductName,
    int Quantity,
    string Unit,
    string ManufacturerName,
    string Note
);

public record TransferPlanDetailDto(
    Guid Id,
    string Code,
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    string Status,
    string Note,
    List<TransferPlanItemDetailDto> Items
);

public record SupplyRequestDetailDto(
    Guid Id,
    string Code,
    Guid WarehouseId,
    string Status,
    string Note,
    DateTimeOffset CreatedAt,
    List<SupplyRequestItemDetailDto> Items,
    List<PurchaseOptionDetailDto> PurchaseOptions,
    List<TransferPlanDetailDto> TransferOrders,
    Guid? PickingNoteId = null
);

public record SupplyRequestListItemDto(
    Guid Id,
    string Code,
    Guid WarehouseId,
    string Status,
    string Note,
    DateTimeOffset CreatedAt,
    int TotalRequested
);

public record GetSupplyRequestByIdQuery(Guid Id) : IRequest<Result<SupplyRequestDetailDto>>;
public record GetPageListSupplyRequestsQuery(int Page = 1, int PageSize = 20) : IRequest<Result<object>>;
