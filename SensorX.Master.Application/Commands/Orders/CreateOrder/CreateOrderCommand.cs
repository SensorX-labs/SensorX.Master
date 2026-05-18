using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Commands.Orders.CreateOrder;

public record CreateOrderCommand(
    Guid QuoteId,
    string? Code,
    Guid CustomerId,
    CustomerInfoDto CustomerInfo,
    SenderInfoDto SenderInfo,
    DateTimeOffset? OrderDate,
    List<OrderItemDto> Items
) : IRequest<Result<Guid>>;

public record CustomerInfoDto(
    string RecipientName,
    string RecipientPhone,
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
