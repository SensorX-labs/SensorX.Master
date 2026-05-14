using MediatR;

namespace SensorX.Master.Application.Commands.Warehouses;

public record DeleteWarehouseCommand(Guid Id) : IRequest<Result>;
