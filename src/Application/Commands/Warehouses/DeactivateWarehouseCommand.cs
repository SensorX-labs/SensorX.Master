using MediatR;

namespace SensorX.Master.Application.Commands.Warehouses;

public record DeactivateWarehouseCommand(Guid Id) : IRequest<Result>;
