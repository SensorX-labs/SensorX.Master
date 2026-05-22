using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Application.Queries.Orders.GetDetailOrderById;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.PaymentAggregate;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Queries.Orders.GetMyOrderById;

public class GetMyOrderByIdHandler(
    IQueryBuilder<Order> orderQueryBuilder,
    IRepository<Payment> paymentRepository,
    IQueryExecutor queryExecutor,
    ICurrentUser currentUser,
    IDataServiceClient dataServiceClient
) : IRequestHandler<GetMyOrderByIdQuery, Result<GetDetailOrderByIdResponse>>
{
    public async Task<Result<GetDetailOrderByIdResponse>> Handle(
        GetMyOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            return Result<GetDetailOrderByIdResponse>.Failure("Nguoi dung chua duoc xac thuc");

        var customerResponse = await dataServiceClient.GetCustomerByAccountIdAsync(currentUser.UserId.Value);
        if (!customerResponse.IsSuccess || customerResponse.Value is null)
            return Result<GetDetailOrderByIdResponse>.Failure(customerResponse.Message ?? "Khong tim thay customer cua nguoi dung hien tai");

        var customerId = new CustomerId(customerResponse.Value.Id);
        var order = await queryExecutor.FirstOrDefaultAsync(
            orderQueryBuilder.QueryAsNoTracking.Where(o => o.Id == new OrderId(request.OrderId) && o.CustomerId == customerId),
            cancellationToken);

        if (order == null)
            return Result<GetDetailOrderByIdResponse>.Failure("Khong tim thay don hang");

        var payment = await queryExecutor.FirstOrDefaultAsync(
            paymentRepository.AsQueryable().Where(p => p.OrderId == order.Id),
            cancellationToken);

        var response = new GetDetailOrderByIdResponse(
            order.Id.Value,
            order.QuoteId.Value,
            order.Code.Value,
            order.CustomerId.Value,
            order.Status.ToString(),
            order.OrderDate,
            order.CustomerInfo.RecipientName,
            order.CustomerInfo.RecipientPhone.Value,
            order.CustomerInfo.CompanyName,
            order.CustomerInfo.Email.Value,
            order.CustomerInfo.Address,
            order.CustomerInfo.TaxCode,
            order.SenderInfo.Name,
            order.SenderInfo.Email.Value,
            order.GetSubtotal().Amount,
            order.GetTotalTax().Amount,
            order.GetGrandTotal().Amount,
            payment?.Id.Value,
            payment?.Status.ToString(),
            payment?.PaymentType.ToString(),
            payment?.PaymentQRURls,
            payment?.Amount.Amount,
            order.Items.Select(i => new OrderItemResponse(
                i.Id.Value,
                i.ProductId.Value,
                i.ProductCode.Value,
                i.ProductName,
                i.Manufacturer,
                i.Unit,
                i.Quantity.Value,
                i.UnitPrice.Amount,
                i.TaxRate.Value,
                i.Note,
                i.GetLineAmount().Amount,
                i.GetTaxAmount().Amount,
                i.GetTotalLineAmount().Amount
            )).ToList()
        );

        return Result<GetDetailOrderByIdResponse>.Success(response);
    }
}
