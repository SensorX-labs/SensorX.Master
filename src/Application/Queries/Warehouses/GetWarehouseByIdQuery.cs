using MediatR;

namespace SensorX.Master.Application.Queries.Warehouses;

public record GetWarehouseByIdQuery(Guid Id) : IRequest<Result<WarehouseDto>>;
