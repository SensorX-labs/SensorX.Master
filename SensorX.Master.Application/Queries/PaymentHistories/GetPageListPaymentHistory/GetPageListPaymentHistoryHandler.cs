using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.PaymentContext.AggregateModels;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.PaymentAggregate;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Application.Queries.PaymentHistories.GetPageListPaymentHistory;

public class GetPageListPaymentHistoryHandler(
    IQueryBuilder<PaymentHistory> queryBuilder,
    IQueryExecutor queryExecutor
) : IRequestHandler<GetPageListPaymentHistoryQuery, Result<OffsetPagedResult<GetPageListPaymentHistoryResponse>>>
{
    public async Task<Result<OffsetPagedResult<GetPageListPaymentHistoryResponse>>> Handle(
        GetPageListPaymentHistoryQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var sourceQuery = queryBuilder.QueryAsNoTracking;

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.Trim().ToLower();
                sourceQuery = sourceQuery.Where(x => 
                    x.Content.ToLower().Contains(search) || 
                    x.AccountNumber.ToLower().Contains(search) || 
                    x.ReferenceCode.ToLower().Contains(search) ||
                    (x.Description != null && x.Description.ToLower().Contains(search))
                );
            }

            if (!string.IsNullOrWhiteSpace(request.Gateway))
            {
                var gateway = request.Gateway.Trim().ToLower();
                sourceQuery = sourceQuery.Where(x => x.Gateway.ToLower().Contains(gateway));
            }

            if (request.PaymentId.HasValue)
            {
                sourceQuery = sourceQuery.Where(x => x.PaymentId == new PaymentId(request.PaymentId.Value));
            }

            if (request.OrderId.HasValue)
            {
                sourceQuery = sourceQuery.Where(x => x.OrderId == new OrderId(request.OrderId.Value));
            }

            if (!string.IsNullOrWhiteSpace(request.Status)
                && Enum.TryParse<PaymentHistoryStatus>(request.Status, true, out var status))
            {
                sourceQuery = sourceQuery.Where(x => x.Status == status);
            }

            var totalCount = await queryExecutor.CountAsync(sourceQuery, cancellationToken);

            var pagedQuery = sourceQuery
                .OrderByDescending(x => x.TransactionDate)
                .ApplyOffsetPagination(request);

            var histories = await queryExecutor.ToListAsync(pagedQuery, cancellationToken);

            var items = histories.Select(x => new GetPageListPaymentHistoryResponse(
                x.Id,
                x.Gateway,
                x.TransactionDate,
                x.SubAccount,
                x.Code,
                x.AccountNumber,
                x.Content,
                x.TransferType,
                x.Description,
                x.TransferAmount,
                x.ReferenceCode,
                x.Accumulated,
                x.Status.ToString(),
                x.PaymentId.Value,
                x.OrderId.Value
            )).ToList();

            var result = new OffsetPagedResult<GetPageListPaymentHistoryResponse>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber ?? 1,
                PageSize = request.PageSize ?? 10
            };

            return Result<OffsetPagedResult<GetPageListPaymentHistoryResponse>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<OffsetPagedResult<GetPageListPaymentHistoryResponse>>.Failure($"Lỗi khi lấy danh sách lịch sử thanh toán: {ex.Message}");
        }
    }
}
