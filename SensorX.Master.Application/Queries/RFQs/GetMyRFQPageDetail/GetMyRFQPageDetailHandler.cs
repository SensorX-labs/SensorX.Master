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
    IQueryBuilder<SaleStaff> _staffBuilder,
    IQueryExecutor _queryExecutor
) : IRequestHandler<GetMyRFQPageDetailQuery, Result<MyRfqDetail>>
{
    public async Task<Result<MyRfqDetail>> Handle(GetMyRFQPageDetailQuery request, CancellationToken cancellationToken)
    {
        var rfqData = await _rfqRepository.GetByIdAsync(new RFQId(request.Id), cancellationToken);

        if (rfqData is null)
        {
            return Result<MyRfqDetail>.Failure("Không tìm thấy yêu cầu báo giá.");
        }

        MyRfqSaleStaff? saleStaff = null;
        if (rfqData.StaffId != null && rfqData.Status != RFQStatus.Draft && rfqData.Status != RFQStatus.Pending)
        {
            var staffQuery = _staffBuilder.QueryAsNoTracking
                .Where(x => x.Id == rfqData.StaffId);
            var staff = await _queryExecutor.FirstOrDefaultAsync(staffQuery, cancellationToken);
            if (staff != null)
            {
                saleStaff = new MyRfqSaleStaff(
                    staff.Id.Value,
                    staff.Name,
                    staff.Phone,
                    staff.Email,
                    staff.AvatarUrl
                );
            }
        }

        var customerQuery = _customerBuilder.QueryAsNoTracking.Where(x => x.Id == rfqData.CustomerId);
        var customer = await _queryExecutor.FirstOrDefaultAsync(customerQuery, cancellationToken);
        MyRfqDetailCustomer? customerDetail = null;
        if (customer != null)
        {
            customerDetail = new MyRfqDetailCustomer(
                customer.Id.Value,
                customer.CompanyName,
                customer.Email.ToString(),
                customer.Phone?.ToString(),
                customer.Address
            );
        }

        var result = new MyRfqDetail(
            rfqData.Id.Value,
            rfqData.Code.ToString(),
            rfqData.Status.ToString(),
            rfqData.CreatedAt,
            saleStaff,
            customerDetail,
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
