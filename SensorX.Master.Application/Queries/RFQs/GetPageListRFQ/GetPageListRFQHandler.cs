using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.Pagination;
using SensorX.Master.Application.Common.QueryExtensions.Search;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Queries.RFQs.GetPageListRFQ;

public class GetPageListRFQHandler(
    IQueryBuilder<RFQ> _RFQQueryBuilder,
    IQueryExecutor _queryExecutor
) : IRequestHandler<GetPageListRFQQuery, Result<RFQCursorPagedResult>>
{
    public async Task<Result<RFQCursorPagedResult>> Handle(
        GetPageListRFQQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var sourceQuery = _RFQQueryBuilder.QueryAsNoTracking.ApplySearch(request.SearchTerm);
            var pagedQuery = sourceQuery.ApplyCursorPagination(
                request,
                x => x.CreatedAt,
                x => x.Id.Value
            )
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id.Value);

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

            var items = await _queryExecutor.ToListAsync(dtoQuery
                .Take(request.PageSize + 1), cancellationToken);

            var hasNext = items.Count > request.PageSize;
            if (hasNext) items.RemoveAt(request.PageSize);

            var result = new RFQCursorPagedResult
            {
                Items = items,
                HasNext = hasNext,
                HasPrevious = request.IsPrevious,
                FirstCreatedAt = items.FirstOrDefault()?.CreatedAt,
                FirstId = items.FirstOrDefault()?.Id,
                LastCreatedAt = items.LastOrDefault()?.CreatedAt,
                LastId = items.LastOrDefault()?.Id
            };

            return Result<RFQCursorPagedResult>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<RFQCursorPagedResult>.Failure(
                $"Lỗi khi lấy danh sách khách hàng: {ex.Message}");
        }
    }
}
