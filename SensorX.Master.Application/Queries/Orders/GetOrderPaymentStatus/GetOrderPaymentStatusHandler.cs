using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.PaymentAggregate;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Application.Queries.Orders.GetOrderPaymentStatus;

public class GetOrderPaymentStatusHandler(
    IQueryBuilder<Payment> queryBuilder,
    IQueryExecutor queryExecutor
) : IRequestHandler<GetOrderPaymentStatusQuery, Result<GetOrderPaymentStatusResponse>>
{
    public async Task<Result<GetOrderPaymentStatusResponse>> Handle(
        GetOrderPaymentStatusQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var payment = await queryExecutor.FirstOrDefaultAsync(
                queryBuilder.QueryAsNoTracking.Where(x => x.OrderId == new OrderId(request.OrderId)),
                cancellationToken);

            if (payment is null)
            {
                return Result<GetOrderPaymentStatusResponse>.Success(
                    new GetOrderPaymentStatusResponse(request.OrderId, false, "Pending"));
            }

            var isPaid = payment.Status == PaymentStatus.Completed;
            return Result<GetOrderPaymentStatusResponse>.Success(
                new GetOrderPaymentStatusResponse(request.OrderId, isPaid, payment.Status.ToString()));
        }
        catch (Exception ex)
        {
            return Result<GetOrderPaymentStatusResponse>.Failure($"Lỗi khi kiểm tra trạng thái thanh toán: {ex.Message}");
        }
    }
}
