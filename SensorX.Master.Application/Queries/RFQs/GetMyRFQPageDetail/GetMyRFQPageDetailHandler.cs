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

        MyRfqSaleStaff? saleStaff = await GetSaleStaff(rfqData, cancellationToken);
        MyRfqDetailCustomer? customerDetail = await GetCustomerDetail(rfqData, cancellationToken);

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

    private async Task<MyRfqSaleStaff?> GetSaleStaff(RFQ rfqData, CancellationToken cancellationToken)
    {
        if (rfqData.StaffId != null && rfqData.Status != RFQStatus.Draft && rfqData.Status != RFQStatus.Pending)
        {
            var staffQuery = _staffBuilder.QueryAsNoTracking.Where(x => x.Id == rfqData.StaffId);
            var staff = await _queryExecutor.FirstOrDefaultAsync(staffQuery, cancellationToken);
            if (staff != null)
            {
                return new MyRfqSaleStaff(
                    staff.Id.Value,
                    staff.Name,
                    staff.Phone,
                    staff.Email,
                    staff.AvatarUrl
                );
            }
        }
        return null;
    }

    private async Task<MyRfqDetailCustomer?> GetCustomerDetail(RFQ rfqData, CancellationToken cancellationToken)
    {
        if (rfqData.CustomerInfo != null && rfqData.Status != RFQStatus.Draft)
        {
            var shippingInfo = new ShippingInfo(
                string.Empty,
                rfqData.CustomerInfo.Phone?.Value ?? string.Empty,
                rfqData.CustomerInfo.Address ?? string.Empty
            );
            return new MyRfqDetailCustomer(
                rfqData.CustomerId.Value,
                rfqData.CustomerInfo.CompanyName,
                rfqData.CustomerInfo.Email.Value,
                rfqData.CustomerInfo.Phone?.Value,
                rfqData.CustomerInfo.Address,
                shippingInfo
            );
        }

        var customerQuery = _customerBuilder.QueryAsNoTracking.Where(x => x.Id == rfqData.CustomerId);
        var customer = await _queryExecutor.FirstOrDefaultAsync(customerQuery, cancellationToken);
        if (customer != null)
        {
            var shippingInfo = new ShippingInfo(
                customer.RecipientName ?? string.Empty,
                customer.RecipientPhone?.Value ?? string.Empty,
                customer.ShippingAddress ?? string.Empty
            );
            return new MyRfqDetailCustomer(
                customer.Id.Value,
                customer.CompanyName,
                customer.Email.Value,
                customer.Phone?.Value,
                customer.Address,
                shippingInfo
            );
        }
        return null;
    }
}
