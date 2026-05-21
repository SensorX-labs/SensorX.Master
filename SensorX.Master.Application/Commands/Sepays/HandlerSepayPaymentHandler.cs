using SensorX.Master.Application.Abstractions.Messaging;

namespace SensorX.Master.Application.Commands.Sepays
{
    public class HandlerSepayPaymentHandler : ICommandHandler<HandlerSepayPaymentCommand, bool>
    {
        public async Task<bool> Handle(HandlerSepayPaymentCommand request, CancellationToken cancellationToken)
        {
            return true;
        }
    }
}