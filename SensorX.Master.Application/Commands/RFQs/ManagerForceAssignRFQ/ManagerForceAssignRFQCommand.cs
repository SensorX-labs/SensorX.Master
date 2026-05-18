using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Commands.RFQs.ManagerForceAssignRFQ
{
    public record ManagerForceAssignRFQCommand(Guid Id, Guid StaffId) : IRequest<Result>;
}