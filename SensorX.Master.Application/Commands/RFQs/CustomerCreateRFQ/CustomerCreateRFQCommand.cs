using System.Collections.Generic;
using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Commands.RFQs.CustomerCreateRFQ
{
    public record CustomerCreateRFQCommand(
        Guid CustomerId,
        string RecipientName,
        string RecipientPhone,
        string CompanyName,
        string Email,
        string Address,
        string TaxCode,
        List<CustomerRFQItemDto> Items
    ) : IRequest<Result<Guid>>;

    public record CustomerRFQItemDto(
        Guid ProductId,
        string ProductName,
        int Quantity,
        string ProductCode,
        string Manufacturer,
        string Unit
    );
}