namespace SensorX.Master.Application.Commands.Quotes.WithDraw;

using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

public record WithDrawCommand(Guid Id) : IRequest<Result>;