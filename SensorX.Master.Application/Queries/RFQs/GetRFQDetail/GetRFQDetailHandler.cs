using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Queries.RFQs.GetRFQDetail;

public class GetRFQDetailHandler(
    IQueryBuilder<RFQ> _rfqQueryBuilder,
    IQueryBuilder<SaleStaff> _saleStaffQueryBuilder,
    IQueryExecutor _queryExecutor
) : IRequestHandler<GetRFQDetailQuery, Result<GetRFQDetailResponse>>
{
    public async Task<Result<GetRFQDetailResponse>> Handle(GetRFQDetailQuery request, CancellationToken cancellationToken)
    {
        var rfqQuery = _rfqQueryBuilder.QueryAsNoTracking
                        .Where(q => q.Id == new RFQId(request.Id));

        var staffQuery = _saleStaffQueryBuilder.QueryAsNoTracking;

        var query = from q in rfqQuery
                    join staff in staffQuery on q.StaffId equals staff.Id into staffGroup
                    from s in staffGroup.DefaultIfEmpty()
                    select new GetRFQDetailResponse
                    (
                        q.Id.Value,
                        q.Code.Value,
                        q.StaffId != null ? q.StaffId.Value : null,
                        s != null ? s.Name : null,
                        q.CustomerId.Value,
                        q.Status.ToString(),
                        q.CreatedAt,
                        q.UpdatedAt,
                        // Flat Customer Info (CompanyName, Phone, Email, Address, TaxCode)
                        q.CustomerInfo == null ? string.Empty : q.CustomerInfo.CompanyName,
                        q.CustomerInfo == null ? string.Empty : q.CustomerInfo.Phone,
                        q.CustomerInfo == null ? string.Empty : q.CustomerInfo.Email,
                        q.CustomerInfo == null ? string.Empty : q.CustomerInfo.Address,
                        q.CustomerInfo == null ? string.Empty : q.CustomerInfo.TaxCode,
                        // Map Items
                        q.Items.Select(i => new RFQItemResponse
                        (
                            i.Id.Value,
                            i.ProductId.Value,
                            i.ProductName,
                            i.ProductCode.Value,
                            i.Quantity.Value,
                            i.Manufacturer,
                            i.Unit
                        )).ToList()
                    );

        var response = await _queryExecutor.FirstOrDefaultAsync(query, cancellationToken);
        if (response == null)
        {
            return Result<GetRFQDetailResponse>.Failure("Không tìm thấy RFQ");
        }
        return Result<GetRFQDetailResponse>.Success(response);
    }
}