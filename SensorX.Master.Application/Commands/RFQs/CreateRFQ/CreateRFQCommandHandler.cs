using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.ValueObjects;
using SensorX.Master.Domain.StrongIDs;
using MediatR;

namespace SensorX.Master.Application.Commands.RFQs.CreateRFQ
{
    public class CreateRFQCommandHandler(
        IRepository<RFQ> _rfqRepository 
    ) : IRequestHandler<CreateRFQCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateRFQCommand request, CancellationToken cancellationToken)
        {
            var customerInfo = new CustomerInfo(
                request.RecipientName,
                Phone.From(request.RecipientPhone),
                request.CompanyName,
                Email.From(request.Email),
                request.Address,
                request.TaxCode
            );

            var rfq = new RFQ(
                RFQId.New(),
                Code.Create("RFQ"),
                new StaffId(request.StaffId),
                new CustomerId(request.CustomerId),
                customerInfo,
                RFQStatus.Pending
            );

            await _rfqRepository.AddAsync(rfq);
            await _rfqRepository.SaveChangesAsync();
            return Result<Guid>.Success(rfq.Id.Value);
        }
    }
}