using System.Text.Json.Serialization;
using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Commands.Quotes.UpdateDraftQuote;

public record UpdateDraftQuoteCommand(
    [property: JsonIgnore] Guid Id,
    string Note,
    List<QuoteItemDto> Items
) : IRequest<Result>;

public record QuoteItemDto
{
    public Guid ProductId { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal TaxRate { get; init; }
}