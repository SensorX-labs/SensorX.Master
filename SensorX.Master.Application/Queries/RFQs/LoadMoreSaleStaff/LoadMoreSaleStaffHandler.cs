using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.QueryExtensions.LoadMore;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.RFQs.LoadMoreSaleStaff;

public sealed class LoadMoreSaleStaffHandler(
    IQueryBuilder<SaleStaff> _staffBuilder,
    IQueryExecutor _queryExecutor
) : IRequestHandler<LoadMoreSaleStaffQuery, Result<LoadMoreSaleStaffResult>>
{
    public async Task<Result<LoadMoreSaleStaffResult>> Handle(LoadMoreSaleStaffQuery request, CancellationToken cancellationToken)
    {
        var query = _staffBuilder.QueryAsNoTracking;

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(x =>
                ((string)x.Code).Contains(request.SearchTerm) ||
                x.Name.Contains(request.SearchTerm) ||
                ((string)x.Email).Contains(request.SearchTerm) ||
                (x.Phone != null && ((string)x.Phone).Contains(request.SearchTerm))
            );
        }

        var loadMoreQuery = query.ApplyLoadMoreWithOrder(
            request.LastValue,
            x => x.Name,
            request.LastId,
            x => (Guid)x.Id
        );


        var pageSize = request.PageSize ?? 10;
        var items = await _queryExecutor.ToListAsync(loadMoreQuery.Take(pageSize + 1), cancellationToken);

        bool hasNext = items.Count > pageSize;
        if (hasNext) items.RemoveAt(items.Count - 1);

        var responseItems = items.Select(x => new LoadMoreSaleStaffResponse(
            (Guid)x.Id,
            (string)x.Code,
            x.Name,
            x.Phone != null ? (string)x.Phone : string.Empty,
            (string)x.Email
        )).ToList();

        var lastItem = responseItems.LastOrDefault();

        var result = new LoadMoreSaleStaffResult
        {
            Items = responseItems,
            LastId = lastItem?.Id,
            LastValue = lastItem?.Name,
            HasNext = hasNext
        };
        return Result<LoadMoreSaleStaffResult>.Success(result);
    }
}