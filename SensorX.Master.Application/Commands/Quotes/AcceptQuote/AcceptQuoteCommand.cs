using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;

namespace SensorX.Master.Application.Commands.Quotes.AcceptQuote;

public record AcceptQuoteCommand(
    QuoteResponseStatus ResponseType = QuoteResponseStatus.Accept,
    PaymentTerm PaymentTerm = PaymentTerm.FullPayment,
    string? ShippingAddress = null,
    string? Feedback = null
) : IRequest<Result>
{
    public Guid QuoteId { get; init; }

    public AcceptQuoteCommand WithId(Guid id) => this with { QuoteId = id };
}
