using System;
using System.Collections.Generic;
using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Application.DTOs;

namespace SensorX.Master.Application.Commands.Warehouses;

public record CreateWarehouseCommand(CreateWarehouseDto Warehouse) : IRequest<Result<Guid>>;
public record DeleteWarehouseCommand(Guid Id) : IRequest<Result>;
public record DeactivateWarehouseCommand(Guid Id) : IRequest<Result>;
public record ActivateWarehouseCommand(Guid Id) : IRequest<Result>;