using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Commands.RFQs.AssignRFQ
{
    public record AssignRFQCommand(Guid RFQId, Guid StaffId) : IRequest<Result<Guid>>;
}