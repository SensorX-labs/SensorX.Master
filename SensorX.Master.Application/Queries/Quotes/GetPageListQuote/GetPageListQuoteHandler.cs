using MediatR;
using Microsoft.EntityFrameworkCore;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Queries.Quotes.GetPageListQuote;

public class GetPageListQuoteHandler(
    IRepository<Quote> _quoteRepository
) : IRequestHandler<GetPageListQuoteQuery, Result<PaginatedResult<GetPageListQuoteResponse>>>
{
    public async Task<Result<PaginatedResult<GetPageListQuoteResponse>>> Handle(
        GetPageListQuoteQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Đảm bảo giá trị hợp lệ cho phân trang
            var pageIndex = request.PageIndex <= 0 ? 1 : request.PageIndex;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            var query = _quoteRepository.AsQueryable()
                .Include(q => q.LineItems)
                .AsNoTracking();

            // Lọc theo từ khóa tìm kiếm
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = $"%{request.SearchTerm.Trim()}%";
                query = query.Where(q =>
                    EF.Functions.ILike((string)(object)q.Code, searchTerm) ||
                    EF.Functions.ILike(q.CustomerInfo.RecipientName, searchTerm) ||
                    EF.Functions.ILike(q.CustomerInfo.CompanyName, searchTerm)
                );
            }

            // Lọc theo khách hàng
            if (request.CustomerId.HasValue)
            {
                query = query.Where(q => q.CustomerId.Value == request.CustomerId.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var skip = (pageIndex - 1) * pageSize;
            var quotes = await query
                .OrderByDescending(q => q.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var quoteDtos = quotes.Select(q => new GetPageListQuoteResponse
            {
                Id = q.Id.Value,
                Code = q.Code.Value,
                Status = q.Status.ToString(),
                QuoteDate = q.QuoteDate,
                CustomerId = q.CustomerId.Value,
                RecipientName = q.CustomerInfo.RecipientName,
                CompanyName = q.CustomerInfo.CompanyName,
                GrandTotal = q.GetGrandTotal().Amount,
                ItemCount = q.LineItems.Count,
                CreatedAt = q.CreatedAt
            }).ToList();

            var paginatedResult = new PaginatedResult<GetPageListQuoteResponse>(
                quoteDtos,
                totalCount,
                pageIndex,
                pageSize
            );

            return Result<PaginatedResult<GetPageListQuoteResponse>>.Success(paginatedResult);
        }
        catch (Exception ex)
        {
            return Result<PaginatedResult<GetPageListQuoteResponse>>.Failure(
                $"Lỗi khi lấy danh sách báo giá: {ex.Message}");
        }
    }
}
