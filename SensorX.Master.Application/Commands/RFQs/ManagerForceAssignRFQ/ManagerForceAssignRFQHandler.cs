using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Application.Commands.RFQs.ManagerForceAssignRFQ
{
    public class ManagerForceAssignRFQHandler(
        IRepository<RFQ> _rfqRepository
    ) : IRequestHandler<ManagerForceAssignRFQCommand, Result>
    {
        public async Task<Result> Handle(ManagerForceAssignRFQCommand request, CancellationToken cancellationToken)
        {
            var rfqId = new RFQId(request.Id);
            var rfq = await _rfqRepository.GetByIdAsync(rfqId, cancellationToken);

            if (rfq is null)
                return Result.Failure("Không tìm thấy RFQ!");

            var staffId = new StaffId(request.StaffId);
            rfq.ForceAssign(staffId);

            await _rfqRepository.SaveChangesAsync(cancellationToken);

            return Result.Success("Chỉ định nhân viên thành công!");
        }
    }
}