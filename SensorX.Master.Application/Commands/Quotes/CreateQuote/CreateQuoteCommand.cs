using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Commands.Quotes.CreateQuote;

public record CreateQuoteCommand(
    Guid RFQId,
    Guid CustomerId,
    string RecipientName,
    string RecipientPhone,
    string CompanyName,
    string Email,
    string Address,
    string TaxCode,
    string? Note,
    DateTimeOffset QuoteDate,
    string ShippingAddress,
    int PaymentTermDays,
    List<CreateQuoteItemCommand> Items
) : IRequest<Result<Guid>>;

public record CreateQuoteItemCommand(
    Guid ProductId,
    string ProductCode,
    string Manufacturer,
    string Unit,
    int Quantity,
    decimal UnitPrice,
    decimal TaxRate
);
