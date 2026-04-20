using System.Collections.Generic;
using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Commands.RFQs.CreateRFQ
{
    public record CreateRFQCommand(
        Guid CustomerId,
        string RecipientName,
        string RecipientPhone,
        string CompanyName,
        string Email,
        string Address,
        string TaxCode,
        List<RFQItemDto> Items
    ) : IRequest<Result<Guid>>;

    public record RFQItemDto(
        Guid ProductId,
        string ProductName,
        int Quantity,
        string ProductCode,
        string Manufacturer,
        string Unit
    );
}