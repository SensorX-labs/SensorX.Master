using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.Orders.GetDetailOrderById;

public record GetDetailOrderByIdQuery(Guid OrderId) : IRequest<Result<GetDetailOrderByIdResponse>>;
