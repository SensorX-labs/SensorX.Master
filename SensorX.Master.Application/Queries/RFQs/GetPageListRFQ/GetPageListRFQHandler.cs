using System.Linq;
using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;
using SensorX.Master.Application.Common.QueryExtensions.Search;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Application.Common.ResponseClient;
using System.Text.Json;
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
            var sourceQuery = _RFQQueryBuilder.QueryAsNoTracking.Where(x => x.Status != RFQStatus.Draft);
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
                .OrderByDescending(x => x.UpdatedAt)
                .ThenByDescending(x => x.Id)
                .ApplyOffsetPagination(request);

            var staffQuery = _staffQueryBuilder.QueryAsNoTracking;

            var dtoQuery = from rfq in pagedQuery
                           join staff in staffQuery on rfq.StaffId equals staff.Id into staffGroup
                           from s in staffGroup.DefaultIfEmpty()
                           select new
                           {
                               Id = rfq.Id.Value,
                               Code = rfq.Code.Value,
                               Status = rfq.Status.ToString(),
                               CompanyName = rfq.CustomerInfo == null ? string.Empty : rfq.CustomerInfo.CompanyName,
                               Phone = rfq.CustomerInfo == null ? string.Empty : rfq.CustomerInfo.Phone,
                               CreatedAt = rfq.CreatedAt,
                               UpdatedAt = rfq.UpdatedAt,
                               StaffId = rfq.StaffId != null ? rfq.StaffId.Value : (Guid?)null,
                               StaffName = s != null ? s.Name : null,
                               ItemCount = rfq.Items.Count,
                               LatestLogJson = rfq.AllocationLogs.OrderByDescending(a => a.Round).Select(a => a.SnapshotJson).FirstOrDefault()
                           };

            var rawItems = await _queryExecutor.ToListAsync(dtoQuery, cancellationToken);

            var items = new List<GetPageListRFQResponse>();
            foreach (var item in rawItems)
            {
                double? finalScore = null;
                if (!string.IsNullOrEmpty(item.LatestLogJson) && item.StaffId.HasValue)
                {
                    try
                    {
                        var snapshots = JsonSerializer.Deserialize<List<SensorX.Master.Application.Services.AIAssignment.Models.AllocationSnapshot>>(item.LatestLogJson);
                        var winnerSnapshot = snapshots?.FirstOrDefault(s => s.StaffId == item.StaffId.Value);
                        if (winnerSnapshot != null)
                        {
                            finalScore = winnerSnapshot.FinalScore;
                        }
                    }
                    catch { /* ignore json parse errors */ }
                }

                items.Add(new GetPageListRFQResponse(
                    item.Id,
                    item.Code,
                    item.Status,
                    item.CompanyName,
                    item.Phone,
                    item.CreatedAt,
                    item.UpdatedAt,
                    item.StaffId,
                    item.StaffName,
                    item.ItemCount,
                    finalScore
                ));
            }

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
