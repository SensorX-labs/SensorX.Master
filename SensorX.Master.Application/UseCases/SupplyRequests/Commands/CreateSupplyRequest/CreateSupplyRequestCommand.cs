using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.UseCases.SupplyRequests.Commands.CreateSupplyRequest;

public record SupplyRequestItemDto(
    Guid ProductId,
    int RequestedQuantity
);

public record CreateSupplyRequestCommand(
    string Code,
    Guid WarehouseId,
    string Note,
    List<SupplyRequestItemDto> Items
) : IRequest<Result<Guid>>;
