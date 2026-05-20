using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Commands.Quotes.CreateDraftQuote;

public record CreateDraftQuoteCommand(
    Guid RFQId,
    DateTimeOffset QuoteDate,
    ShippingInfo ShippingInfo,
    string Note,
    List<QuoteItemDto> Items
) : IRequest<Result<Guid>>;

public sealed record ShippingInfo(
    string RecipientName,
    string RecipientPhone,
    string ShippingAddress
);

public record QuoteItemDto
{
    public Guid ProductId { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal TaxRate { get; init; }
}