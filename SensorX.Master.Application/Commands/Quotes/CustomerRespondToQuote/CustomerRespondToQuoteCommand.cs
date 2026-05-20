using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;

namespace SensorX.Master.Application.Commands.Quotes.CustomerRespondToQuote;

public record CustomerRespondToQuoteCommand(
    QuoteResponseStatus ResponseType,
    PaymentTerm PaymentTerm,
    string? ShippingAddress = null,
    string? Feedback = null
) : IRequest<Result>
{
    public Guid QuoteId { get; init; }

    public CustomerRespondToQuoteCommand WithId(Guid id) => this with { QuoteId = id };
}
