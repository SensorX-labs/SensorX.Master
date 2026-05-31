using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.PaymentContext.AggregateModels;

namespace SensorX.Master.Application.Queries.PaymentHistories.GetDetailPaymentHistory;

public class GetDetailPaymentHistoryHandler(
    IQueryBuilder<PaymentHistory> queryBuilder,
    IQueryExecutor queryExecutor
) : IRequestHandler<GetDetailPaymentHistoryQuery, Result<GetDetailPaymentHistoryResponse>>
{
    public async Task<Result<GetDetailPaymentHistoryResponse>> Handle(
        GetDetailPaymentHistoryQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var history = await queryExecutor.FirstOrDefaultAsync(
                queryBuilder.QueryAsNoTracking.Where(x => x.Id == request.Id),
                cancellationToken);

            if (history == null)
            {
                return Result<GetDetailPaymentHistoryResponse>.Failure("Không tìm thấy lịch sử thanh toán");
            }

            var response = new GetDetailPaymentHistoryResponse(
                history.Id,
                history.Gateway,
                history.TransactionDate,
                history.SubAccount,
                history.Code,
                history.AccountNumber,
                history.Content,
                history.TransferType,
                history.Description,
                history.TransferAmount,
                history.ReferenceCode,
                history.Accumulated,
                history.Status.ToString(),
                history.PaymentId.Value,
                history.OrderId.Value
            );

            return Result<GetDetailPaymentHistoryResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<GetDetailPaymentHistoryResponse>.Failure($"Lỗi khi lấy chi tiết lịch sử thanh toán: {ex.Message}");
        }
    }
}
