using MediatR;

namespace SensorX.Master.Application.UseCases.Orders.Commands.CreateOrder;

public record CreateOrderCommand(
    Guid QuoteId,
    string Code,
    Guid CustomerId,
    CustomerInfoDto CustomerInfo,
    SenderInfoDto SenderInfo,
    DateTimeOffset OrderDate
) : IRequest<Guid>;

public record CustomerInfoDto(
    string RecipientName,
    string RecipientPhone,
    string ShippingAddress,
    string CompanyName,
    string Email,
    string Address,
    string TaxCode
);

public record SenderInfoDto(
    string SenderName,
    string SenderEmail
);
