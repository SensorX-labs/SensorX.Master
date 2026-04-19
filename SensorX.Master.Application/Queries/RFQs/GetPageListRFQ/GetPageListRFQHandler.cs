using MediatR;
using Microsoft.EntityFrameworkCore;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Queries.RFQs.GetPageListRFQ;

public class GetPageListRFQHandler(
    IRepository<RFQ> _rfqRepository
) : IRequestHandler<GetPageListRFQQuery, Result<PaginatedResult<GetPageListRFQResponse>>>
{
    public async Task<Result<PaginatedResult<GetPageListRFQResponse>>> Handle(
        GetPageListRFQQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Đảm bảo giá trị hợp lệ cho phân trang
            var pageIndex = request.PageIndex <= 0 ? 1 : request.PageIndex;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            var query = _rfqRepository.AsQueryable()
                .Include(r => r.Items)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = $"%{request.SearchTerm.Trim()}%";
                query = query.Where(r =>
                    EF.Functions.ILike((string)(object)r.Code, searchTerm) ||
                    EF.Functions.ILike(r.CustomerInfo.RecipientName, searchTerm) ||
                    EF.Functions.ILike((string)(object)r.CustomerInfo.RecipientPhone, searchTerm) ||
                    EF.Functions.ILike(r.CustomerInfo.CompanyName, searchTerm)
                );
            }

            if (request.CustomerId.HasValue)
            {
                query = query.Where(r => r.CustomerId.Value == request.CustomerId.Value);
            }

            if (request.StaffId.HasValue)
            {
                query = query.Where(r => r.StaffId != null && r.StaffId.Value == request.StaffId.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var skip = (pageIndex - 1) * pageSize;
            var rfqs = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var rfqDtos = rfqs.Select(r => new GetPageListRFQResponse
            {
                Id = r.Id.Value,
                Code = r.Code.Value,
                Status = r.Status.ToString(),
                RecipientName = r.CustomerInfo.RecipientName,
                RecipientPhone = r.CustomerInfo.RecipientPhone.Value,
                CompanyName = r.CustomerInfo.CompanyName,
                CreatedAt = r.CreatedAt,
                StaffId = r.StaffId?.Value,
                CustomerId = r.CustomerId.Value,
                ItemCount = r.Items.Count
            }).ToList();

            var paginatedResult = new PaginatedResult<GetPageListRFQResponse>(
                rfqDtos,
                totalCount,
                pageIndex,
                pageSize
            );

            return Result<PaginatedResult<GetPageListRFQResponse>>.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            return Result<PaginatedResult<GetPageListRFQResponse>>.Failure(
                $"Lỗi khi lấy danh sách RFQ: {ex.Message}");
        }
    }
}
