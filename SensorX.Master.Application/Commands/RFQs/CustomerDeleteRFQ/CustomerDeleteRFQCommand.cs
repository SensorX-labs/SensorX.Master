using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
namespace SensorX.Master.Application.Commands.RFQs.CustomerDeleteRFQ;

public record CustomerDeleteRFQCommand(Guid Id) : IRequest<Result>;