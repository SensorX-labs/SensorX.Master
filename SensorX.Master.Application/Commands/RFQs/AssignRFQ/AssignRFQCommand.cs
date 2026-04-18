using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Commands.RFQs.AssignRFQ
{
    public class AssignRFQCommand : IRequest<Result<Guid>>
    {
        public Guid RFQId { get; set; }
        public Guid StaffId { get; set; }
    }
}