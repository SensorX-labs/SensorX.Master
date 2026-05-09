using MediatR;

namespace SensorX.Master.Application.UseCases.TransferOrders.Commands.CreateTransferOrder;

public record CreateTransferOrderCommand(
    string Code,
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    string Note
) : IRequest<Guid>;
