using System;
using System.Collections.Generic;
using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Application.DTOs;

namespace SensorX.Master.Application.Queries.Warehouses;

public record GetWarehouseByIdQuery(Guid Id) : IRequest<Result<WarehouseDto>>;
public record GetAllWarehousesQuery : IRequest<Result<List<WarehouseDto>>>;