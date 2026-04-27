using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;

namespace SensorX.Master.Application.Commands.Quotes.AcceptQuote;

public record AcceptQuoteCommand(
    QuoteResponseStatus ResponseType,
    PaymentTerm PaymentTerm,
    string ShippingAddress,
    string? Feedback
) : IRequest<Result>
{
    public Guid QuoteId { get; init; }

    public AcceptQuoteCommand WithId(Guid id) => this with { QuoteId = id };
}
