using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Application.Commands.RFQs.AssignRFQ
{
    public class AssignRFQHandler(
        IRepository<RFQ> _rfqRepository
    ) : IRequestHandler<AssignRFQCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(AssignRFQCommand request, CancellationToken cancellationToken)
        {
            var rfqId = new RFQId(request.RFQId);
            var rfq = await _rfqRepository.GetByIdAsync(rfqId, cancellationToken);
            
            if (rfq is null)
            {
                return Result<Guid>.Failure("Không tìm thấy RFQ");
            }

            var staffId = new StaffId(request.StaffId);
            rfq.Assign(staffId);

            await _rfqRepository.UpdateAsync(rfq, cancellationToken);
            await _rfqRepository.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(rfq.Id.Value);
        }
    }
}