using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Commands.Orders.CancelOrder;

public record CancelOrderCommand(Guid OrderId) : IRequest<Result<bool>>;
