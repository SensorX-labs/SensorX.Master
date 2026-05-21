using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Commands.Quotes.CreateDraftQuote;

public record CreateDraftQuoteCommand(
    Guid RfqId,
    string Note,
    List<QuoteItemDto> Items
) : IRequest<Result<Guid>>;

public record QuoteItemDto
{
    public Guid ProductId { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal TaxRate { get; init; }
}