using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Commands.RFQs.RejectRFQ
{
    public class RejectRFQHandler(
        IRepository<RFQ> _rfqRepository
    ) : IRequestHandler<RejectRFQCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(RejectRFQCommand request, CancellationToken cancellationToken)
        {
            var rfqId = new RFQId(request.RFQId);
            var rfq = await _rfqRepository.GetByIdAsync(rfqId, cancellationToken);
            if (rfq is null)
                return Result<Guid>.Failure("Không tìm thấy RFQ");

            rfq.StaffReject();
            await _rfqRepository.UpdateAsync(rfq, cancellationToken);
            return Result<Guid>.Success(rfq.Id.Value);
        }
    }
}