using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Commands.RFQs.AcceptRFQ
{
    public record AcceptRFQCommand(Guid RFQId) : IRequest<Result<Guid>>;
}