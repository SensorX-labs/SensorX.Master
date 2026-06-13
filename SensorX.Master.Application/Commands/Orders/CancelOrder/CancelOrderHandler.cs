using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.PaymentAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Application.Commands.Orders.CancelOrder;

public class CancelOrderHandler(
    IRepository<Order> orderRepository,
    IRepository<Payment> paymentRepository,
    IRepository<Invoice> invoiceRepository,
    ICurrentUser currentUser,
    IDataServiceClient dataServiceClient,
    IQueryExecutor queryExecutor,
    IUnitOfWork unitOfWork
) : IRequestHandler<CancelOrderCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            return Result<bool>.Failure("Người dùng chưa được xác thực.");

        var customerResponse = await dataServiceClient.GetCustomerByAccountIdAsync(currentUser.UserId.Value);
        if (!customerResponse.IsSuccess || customerResponse.Value is null)
            return Result<bool>.Failure(customerResponse.Message ?? "Không tìm thấy khách hàng của người dùng hiện tại.");

        var customerId = new CustomerId(customerResponse.Value.Id);
        var orderId = new OrderId(request.OrderId);

        var order = await queryExecutor.FirstOrDefaultAsync(
            orderRepository.AsQueryable()
                .Where(o => o.Id == orderId && o.CustomerId == customerId),
            cancellationToken);

        if (order is null)
            return Result<bool>.Failure("Không tìm thấy đơn hàng.");

        if (order.Status != OrderStatus.PendingPayment)
            return Result<bool>.Failure("Chỉ có thể hủy đơn hàng ở trạng thái chờ thanh toán.");

        // Cancel order
        order.Cancel();
        await orderRepository.Update(order, cancellationToken);

        // Cancel payment (guaranteed to exist)
        var payment = await queryExecutor.FirstOrDefaultAsync(
            paymentRepository.AsQueryable().Where(p => p.OrderId == orderId),
            cancellationToken);

        if (payment is not null)
        {
            payment.Cancel();
            await paymentRepository.Update(payment, cancellationToken);
        }

        // Cancel invoice (guaranteed to exist)
        var invoice = await queryExecutor.FirstOrDefaultAsync(
            invoiceRepository.AsQueryable().Where(i => i.OrderId == orderId),
            cancellationToken);

        if (invoice is not null)
        {
            invoice.Cancel();
            await invoiceRepository.Update(invoice, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
