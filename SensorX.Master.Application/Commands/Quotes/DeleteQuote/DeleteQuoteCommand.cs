using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Commands.Quotes.DeleteQuote;

public sealed record DeleteQuoteCommand(Guid Id) : IRequest<Result>;