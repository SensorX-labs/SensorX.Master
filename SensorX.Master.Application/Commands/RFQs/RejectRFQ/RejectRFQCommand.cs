using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Commands.RFQs.RejectRFQ
{
    public class RejectRFQCommand : IRequest<Result<Guid>>
    {
        public Guid RFQId { get; set; }
    }
}