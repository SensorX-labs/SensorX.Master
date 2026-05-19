using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Commands.RFQs.StaffRejectRFQ
{
    public class StaffRejectRFQHandler(
        IRepository<RFQ> _rfqRepository
    ) : IRequestHandler<StaffRejectRFQCommand, Result>
    {
        public async Task<Result> Handle(StaffRejectRFQCommand request, CancellationToken cancellationToken)
        {
            var rfqId = new RFQId(request.Id);
            var rfq = await _rfqRepository.GetByIdAsync(rfqId, cancellationToken);
            if (rfq is null)
                return Result.Failure("Không tìm thấy RFQ!");

            rfq.Reject();
            await _rfqRepository.SaveChangesAsync(cancellationToken);
            return Result.Success("Từ chối RFQ thành công!");
        }
    }
}