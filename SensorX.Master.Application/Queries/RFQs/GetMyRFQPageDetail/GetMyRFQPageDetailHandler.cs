using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Queries.RFQs.GetMyRFQPageDetail;

public class GetMyRFQPageDetailHandler(
    IRepository<RFQ> _rfqRepository,
    IQueryBuilder<Customer> _customerBuilder,
    IQueryExecutor _queryExecutor
) : IRequestHandler<GetMyRFQPageDetailQuery, Result<MyRfqDetail>>
{
    public async Task<Result<MyRfqDetail>> Handle(GetMyRFQPageDetailQuery request, CancellationToken cancellationToken)
    {
        // 1. Lấy thông tin RFQ và Items thông qua Explicit Join
        var rfqData = await _rfqRepository.GetByIdAsync(new RFQId(request.Id), cancellationToken);

        if (rfqData is null)
        {
            return Result<MyRfqDetail>.Failure("Không tìm thấy yêu cầu báo giá.");
        }

        // 2. Lấy thông tin Customer gốc
        var customerQuery = _customerBuilder.QueryAsNoTracking
            .Where(x => x.Id == rfqData.CustomerId);

        var customer = await _queryExecutor.FirstOrDefaultAsync(customerQuery, cancellationToken);

        // 3. Map sang MyRfqDetail
        var result = new MyRfqDetail(
            rfqData.Id.Value,
            rfqData.Code.ToString(),
            rfqData.Status.ToString(),
            rfqData.CreatedAt,
            rfqData.CustomerId.Value,
            rfqData.CustomerInfo.RecipientName,
            rfqData.CustomerInfo.RecipientPhone.ToString(),
            rfqData.CustomerInfo.Email.ToString(),
            rfqData.CustomerInfo.Address,
            rfqData.CustomerInfo.CompanyName,
            customer != null ? new MyRfqDetailCustomer(
                customer.Id.Value,
                customer.CompanyName,
                customer.Email.ToString(),
                customer.Phone?.ToString(),
                customer.Address
            ) : null,
            [.. rfqData.Items.Select(x => new MyRfqDetailItem(
                x.ProductId.Value,
                x.ProductName,
                x.ProductCode.ToString(),
                x.Quantity.Value,
                x.Unit
            ))]
        );

        return Result<MyRfqDetail>.Success(result);
    }
}
