using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Application.Queries.Orders.GetDetailOrderById;

namespace SensorX.Master.Application.Queries.Orders.GetMyOrderById;

public record GetMyOrderByIdQuery(Guid OrderId) : IRequest<Result<GetDetailOrderByIdResponse>>;
