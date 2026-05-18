using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Commands.RFQs.StaffRejectRFQ
{
    public record StaffRejectRFQCommand(Guid Id) : IRequest<Result>;
}