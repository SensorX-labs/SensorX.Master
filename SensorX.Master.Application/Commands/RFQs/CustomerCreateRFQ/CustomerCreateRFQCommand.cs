using System.Collections.Generic;
using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Commands.RFQs.CustomerCreateRFQ
{
    public record CustomerCreateRFQCommand(
        Guid CustomerId,
        List<CustomerRFQItemDto> Items
    ) : IRequest<Result<Guid>>;

    public record CustomerRFQItemDto(
        Guid ProductId,
        int Quantity
    );
}