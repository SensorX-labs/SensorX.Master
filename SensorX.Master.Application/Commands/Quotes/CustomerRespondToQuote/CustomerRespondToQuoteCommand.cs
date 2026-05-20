using System.Text.Json.Serialization;
using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;

namespace SensorX.Master.Application.Commands.Quotes.CustomerRespondToQuote;

public record CustomerRespondToQuoteCommand(
    [property: JsonIgnore] Guid Id,
    QuoteResponseStatus ResponseType,
    PaymentTerm PaymentTerm,
    string ShippingAddress,
    string RecipientName,
    string RecipientPhone,
    string? Feedback
) : IRequest<Result>;