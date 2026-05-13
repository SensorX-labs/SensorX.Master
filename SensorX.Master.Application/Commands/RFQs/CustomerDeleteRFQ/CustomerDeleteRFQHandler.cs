namespace SensorX.Master.Application.Commands.RFQs.CustomerDeleteRFQ;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

public class CustomerDeleteRFQCommandHandler(
    IRepository<RFQ> _rfqRepository
) : IRequestHandler<CustomerDeleteRFQCommand, Result>
{
    public async Task<Result> Handle(CustomerDeleteRFQCommand request, CancellationToken cancellationToken)
    {
        var rfq = await _rfqRepository.GetByIdAsync(new RFQId(request.Id), cancellationToken);
        if (rfq is null)
        {
            return Result.Failure("Không tìm thấy RFQ.");
        }

        if (rfq.Status != RFQStatus.Draft)
        {
            return Result.Failure("RFQ không ở trạng thái nháp, không thể xóa.");
        }

        await _rfqRepository.DeleteAsync(rfq, cancellationToken);

        return Result.Success("Xóa yêu cầu báo giá thành công.");
    }
}