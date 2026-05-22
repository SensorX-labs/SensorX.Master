namespace SensorX.Master.Application.Commands.Quotes.RejectQuote;

using System.Text.Json.Serialization;
using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

public record RejectQuoteCommand([property: JsonIgnore] Guid Id, string Reason) : IRequest<Result>;