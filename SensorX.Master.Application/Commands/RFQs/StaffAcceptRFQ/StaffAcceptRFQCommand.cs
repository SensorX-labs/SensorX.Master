using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Commands.RFQs.StaffAcceptRFQ
{
    public record StaffAcceptRFQCommand(Guid Id) : IRequest<Result>;
}