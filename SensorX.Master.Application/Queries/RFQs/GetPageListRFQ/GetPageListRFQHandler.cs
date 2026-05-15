using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;
using SensorX.Master.Application.Common.QueryExtensions.Search;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;

namespace SensorX.Master.Application.Queries.RFQs.GetPageListRFQ;

public sealed class GetPageListRFQHandler(
    IQueryBuilder<RFQ> _RFQQueryBuilder,
    IQueryBuilder<SaleStaff> _staffQueryBuilder,
    IQueryExecutor _queryExecutor,
    ICurrentUser _currentUser
) : IRequestHandler<GetPageListRFQQuery, Result<GetPageListRFQResult>>
{
    public async Task<Result<GetPageListRFQResult>> Handle(
        GetPageListRFQQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var sourceQuery = _RFQQueryBuilder.QueryAsNoTracking;
            if (_currentUser.Role == Role.SaleStaff)
            {
                var queryStaffId = _staffQueryBuilder.QueryAsNoTracking.Where(s => s.AccountId == _currentUser.UserId).Select(s => s.Id);
                var staffId = await _queryExecutor.FirstOrDefaultAsync(queryStaffId, cancellationToken);
                sourceQuery = sourceQuery.Where(r => r.StaffId == staffId);
            }

            if (request.Status != null)
            {
                sourceQuery = sourceQuery.Where(r => r.Status == request.Status);
            }

            sourceQuery = sourceQuery.ApplySearch(request.SearchTerm);

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

            var result = new GetPageListRFQResult
            {
                Items = items,
                PageNumber = request.PageNumber ?? 1,
                PageSize = request.PageSize ?? 10,
                TotalCount = totalCount
            };

            return Result<GetPageListRFQResult>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<GetPageListRFQResult>.Failure(
                $"Lỗi khi lấy danh sách yêu cầu báo giá: {ex.Message}");
        }
    }
}
