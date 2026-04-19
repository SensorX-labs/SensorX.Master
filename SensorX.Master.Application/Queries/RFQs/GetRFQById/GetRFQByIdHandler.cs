using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Queries.RFQs.GetRFQById;

public class GetRFQByIdHandler(
    IRepository<RFQ> _rfqRepository
) : IRequestHandler<GetRFQByIdQuery, Result<GetRFQByIdResponse>>
{
    public async Task<Result<GetRFQByIdResponse>> Handle(GetRFQByIdQuery request, CancellationToken cancellationToken)
    {
        var rfq = await _rfqRepository.GetByIdAsync(new RFQId(request.RFQId), cancellationToken);
        if (rfq == null)
        {
            return Result<GetRFQByIdResponse>.Failure("Không tìm thấy RFQ");
        }

        var response = new GetRFQByIdResponse
        {
            Id = rfq.Id.Value,
            Code = rfq.Code.Value,
            StaffId = rfq.StaffId?.Value,
            CustomerId = rfq.CustomerId.Value,
            Status = rfq.Status.ToString(),
            CreatedAt = rfq.CreatedAt,
            // Map Customer Info
            RecipientName = rfq.CustomerInfo.RecipientName,
            RecipientPhone = rfq.CustomerInfo.RecipientPhone.Value,
            CompanyName = rfq.CustomerInfo.CompanyName,
            Email = rfq.CustomerInfo.Email.Value,
            Address = rfq.CustomerInfo.Address,
            TaxCode = rfq.CustomerInfo.TaxCode,
            // Map Items
            Items = rfq.Items.Select(i => new RFQItemResponse
            {
                Id = i.Id.Value,
                ProductId = i.ProductId.Value,
                ProductName = i.ProductName,
                ProductCode = i.ProductCode.Value,
                Quantity = i.Quantity.Value,
                Manufacturer = i.Manufacturer,
                Unit = i.Unit
            }).ToList()
        };

        return Result<GetRFQByIdResponse>.Success(response);
    }
}