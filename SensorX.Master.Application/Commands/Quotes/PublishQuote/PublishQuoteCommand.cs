using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Commands.Quotes.PublishQuote;

public record PublishQuoteCommand(Guid Id) : IRequest<Result>;