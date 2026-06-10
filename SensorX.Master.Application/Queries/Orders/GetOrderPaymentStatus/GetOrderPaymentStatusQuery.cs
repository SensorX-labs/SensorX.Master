using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.Orders.GetOrderPaymentStatus;

public record GetOrderPaymentStatusQuery(Guid OrderId) : IRequest<Result<GetOrderPaymentStatusResponse>>;
