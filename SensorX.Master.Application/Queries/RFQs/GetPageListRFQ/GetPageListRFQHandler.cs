using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;
using SensorX.Master.Application.Common.QueryExtensions.Search;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;

namespace SensorX.Master.Application.Queries.RFQs.GetPageListRFQ;

public class GetPageListRFQHandler(
    IQueryBuilder<RFQ> _RFQQueryBuilder,
    IQueryExecutor _queryExecutor
) : IRequestHandler<GetPageListRFQQuery, Result<RFQOffsetPagedResult>>
{
    public async Task<Result<RFQOffsetPagedResult>> Handle(
        GetPageListRFQQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var sourceQuery = _RFQQueryBuilder.QueryAsNoTracking.ApplySearch(request.SearchTerm);
            var totalCount = await _queryExecutor.CountAsync(sourceQuery, cancellationToken);

            var pagedQuery = sourceQuery
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.Id)
                .ApplyOffsetPagination(request);

            var dtoQuery = pagedQuery.Select(x => new GetPageListRFQResponse(
                x.Id.Value,
                x.Code.Value,
                x.Status.ToString(),
                x.CustomerInfo.RecipientName,
                x.CustomerInfo.RecipientPhone.Value,
                x.CustomerInfo.CompanyName,
                x.CreatedAt,
                x.StaffId != null ? x.StaffId.Value : null,
                x.CustomerId.Value,
                x.Items.Count
            ));

            var items = await _queryExecutor.ToListAsync(dtoQuery, cancellationToken);

            var result = new RFQOffsetPagedResult
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return Result<RFQOffsetPagedResult>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<RFQOffsetPagedResult>.Failure(
                $"Lỗi khi lấy danh sách yêu cầu báo giá: {ex.Message}");
        }
    }
}
