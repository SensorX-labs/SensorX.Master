using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Commands.Quotes.CreateQuote;

public record CreateQuoteCommand(
    Guid RFQId,
    string QuoteCode, // Added QuoteCode
    Guid CustomerId,
    DateTimeOffset QuoteDate,
    string RecipientName,
    string RecipientPhone,
    string ShippingAddress,
    string CompanyName,
    string Email,
    string Address,
    string TaxCode,
    string Note,
    List<QuoteItemDto> Items
) : IRequest<Result<Guid>>;

public record QuoteItemDto
{
    public Guid ProductId { get; init; }
    public string ProductCode { get; init; } = "";
    public string Manufacturer { get; init; } = "";
    public string Unit { get; init; } = "";
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal TaxRate { get; init; }
}