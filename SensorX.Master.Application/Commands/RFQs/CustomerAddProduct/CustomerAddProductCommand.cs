using System.Collections.Generic;
using System.Text.Json.Serialization;
using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Commands.RFQs.CustomerAddProduct;

public record CustomerAddProductCommand(
    [property: JsonIgnore] Guid Id,
    List<CustomerRFQItemDto> Items
) : IRequest<Result>;

public record CustomerRFQItemDto(
    Guid ProductId,
    int Quantity
);