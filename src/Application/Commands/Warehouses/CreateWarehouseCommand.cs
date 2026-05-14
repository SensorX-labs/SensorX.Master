using MediatR;
using SensorX.Master.Application.DTOs;

namespace SensorX.Master.Application.Commands.Warehouses;

public record CreateWarehouseCommand(CreateWarehouseDto Warehouse) : IRequest<Result<Guid>>;
