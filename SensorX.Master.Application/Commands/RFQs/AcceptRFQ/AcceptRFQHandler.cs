using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Commands.RFQs.AcceptRFQ
{
    public class AcceptRFQHandler(
        IRepository<RFQ> _rfqRepository
    ) : IRequestHandler<AcceptRFQCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(AcceptRFQCommand request, CancellationToken cancellationToken)
        {
            var rfqId = new RFQId(request.RFQId);
            var rfq = await _rfqRepository.GetByIdAsync(rfqId, cancellationToken);
            if (rfq == null)
            {
                return Result<Guid>.Failure("Không tìm thấy RFQ");
            }
            
            try 
            {
                rfq.Accept();
                await _rfqRepository.UpdateAsync(rfq, cancellationToken);
                await _rfqRepository.SaveChangesAsync(cancellationToken);
                return Result<Guid>.Success(rfq.Id.Value);
            }
            catch (SensorX.Master.Domain.Common.Exceptions.DomainException ex)
            {
                return Result<Guid>.Failure(ex.Message);
            }
        }
    }
}