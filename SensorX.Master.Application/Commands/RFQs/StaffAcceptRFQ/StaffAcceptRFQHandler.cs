using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Commands.RFQs.StaffAcceptRFQ
{
    public class StaffAcceptRFQHandler(
        IRepository<RFQ> _rfqRepository
    ) : IRequestHandler<StaffAcceptRFQCommand, Result>
    {
        public async Task<Result> Handle(StaffAcceptRFQCommand request, CancellationToken cancellationToken)
        {
            var rfqId = new RFQId(request.Id);
            var rfq = await _rfqRepository.GetByIdAsync(rfqId, cancellationToken);
            if (rfq is null)
                return Result.Failure("Không tìm thấy RFQ!");

            rfq.Accept();
            await _rfqRepository.SaveChangesAsync(cancellationToken);
            return Result.Success("Tiếp nhận RFQ thành công!");
        }
    }
}