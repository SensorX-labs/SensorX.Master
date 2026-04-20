using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.RFQs.GetRFQById;

public record GetRFQByIdQuery(Guid RFQId) : IRequest<Result<GetRFQByIdResponse>>;