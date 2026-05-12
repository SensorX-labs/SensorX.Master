using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Commands.RFQs.CustomerSendRFQ;

public class CustomerSendRFQCommandHandler(
    IRepository<RFQ> _rfqRepository
) : IRequestHandler<CustomerSendRFQCommand, Result>
{
    public async Task<Result> Handle(CustomerSendRFQCommand request, CancellationToken cancellationToken)
    {
        var rfq = await _rfqRepository.GetByIdAsync(new RFQId(request.Id), cancellationToken);
        if (rfq is null)
        {
            return Result.Failure("Không tìm thấy RFQ.");
        }
        rfq.Send();
        await _rfqRepository.SaveChangesAsync(cancellationToken);
        return Result.Success("Gửi yêu cầu báo giá thành công");
    }
}