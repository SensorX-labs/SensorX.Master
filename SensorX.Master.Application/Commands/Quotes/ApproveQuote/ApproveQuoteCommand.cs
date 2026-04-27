using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Commands.Quotes.ApproveQuote;

public record ApproveQuoteCommand(Guid QuoteId) : IRequest<Result>;
