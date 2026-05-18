using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Commands.RFQs.CustomerSendRFQ;

public sealed record CustomerSendRFQCommand(Guid Id) : IRequest<Result>;