using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.UseCases.TransferOrders.Commands.CreateTransferOrder;

public record TransferOrderItemDto(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    string Unit,
    int Quantity,
    string ManufactureName,
    string Note
);

public record CreateTransferOrderCommand(
    string Code,
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    string Note,
    List<TransferOrderItemDto> Items
) : IRequest<Result<Guid>>;
