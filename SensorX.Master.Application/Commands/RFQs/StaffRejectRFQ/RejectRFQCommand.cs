using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Commands.RFQs.RejectRFQ
{
    public record RejectRFQCommand(Guid RFQId) : IRequest<Result<Guid>>;
}