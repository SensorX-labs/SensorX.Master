using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Queries.RFQs.GetRFQById;

public class GetRFQByIdHandler(
    IQueryBuilder<RFQ> _rfqQueryBuilder,
    IQueryExecutor _queryExecutor
) : IRequestHandler<GetRFQByIdQuery, Result<GetRFQByIdResponse>>
{
    public async Task<Result<GetRFQByIdResponse>> Handle(GetRFQByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _rfqQueryBuilder.QueryAsNoTracking
                        .Where(q => q.Id == new RFQId(request.RFQId))
                        .Select(q => new GetRFQByIdResponse
                        (
                            q.Id.Value,
                            q.Code.Value,
                            q.StaffId != null ? q.StaffId.Value : null,
                            q.CustomerId.Value,
                            q.Status.ToString(),
                            q.CreatedAt,
                            // Map Customer Info
                            q.CustomerInfo.RecipientName,
                            q.CustomerInfo.RecipientPhone.Value,
                            q.CustomerInfo.CompanyName,
                            q.CustomerInfo.Email.Value,
                            q.CustomerInfo.Address,
                            q.CustomerInfo.TaxCode,
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
                        ));

        var response = await _queryExecutor.FirstOrDefaultAsync(query, cancellationToken);
        if (response == null)
        {
            return Result<GetRFQByIdResponse>.Failure("Không tìm thấy RFQ");
        }
        return Result<GetRFQByIdResponse>.Success(response);
    }
}