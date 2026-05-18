using MediatR;

namespace SensorX.Master.Application.UseCases.Orders.Commands.CreateOrder;

public record CreateOrderCommand(
    Guid QuoteId,
    string Code,
    Guid CustomerId,
    CustomerInfoDto CustomerInfo,
    SenderInfoDto SenderInfo,
    DateTimeOffset OrderDate,
    List<OrderItemDto> Items
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

public record OrderItemDto(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    string Manufacturer,
    string Unit,
    int Quantity,
    decimal UnitPrice,
    decimal TaxRate,
    string? Note
);
