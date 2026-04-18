using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Commands.RFQs.AcceptRFQ
{
    public class AcceptRFQCommand : IRequest<Result<Guid>>
    {
        public Guid RFQId { get; set; }
    }
}