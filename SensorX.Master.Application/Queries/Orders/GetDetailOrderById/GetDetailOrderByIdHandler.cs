using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Application.Queries.Orders.GetDetailOrderById;

public class GetDetailOrderByIdHandler(
    IQueryBuilder<Order> orderQueryBuilder,
    IQueryExecutor queryExecutor
) : IRequestHandler<GetDetailOrderByIdQuery, Result<GetDetailOrderByIdResponse>>
{
    public async Task<Result<GetDetailOrderByIdResponse>> Handle(
        GetDetailOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var order = await queryExecutor.FirstOrDefaultAsync(
                orderQueryBuilder.QueryAsNoTracking.Where(o => o.Id == new OrderId(request.OrderId)),
                cancellationToken);

            if (order == null)
            {
                return Result<GetDetailOrderByIdResponse>.Failure("Khong tim thay don hang");
            }

            var response = new GetDetailOrderByIdResponse(
                order.Id.Value,
                order.QuoteId.Value,
                order.Code.Value,
                order.CustomerId.Value,
                order.Status.ToString(),
                order.OrderDate,

                order.DeliveryInfo.RecipientName,
                order.DeliveryInfo.RecipientPhone.Value,
                order.DeliveryInfo.CompanyName,
                order.DeliveryInfo.Email.Value,
                order.DeliveryInfo.ShippingAddress,
                order.DeliveryInfo.TaxCode,

                order.SenderInfo.Name,
                order.SenderInfo.Email.Value,

                order.GetSubtotal().Amount,
                order.GetTotalTax().Amount,
                order.GetGrandTotal().Amount,

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
        catch (Exception ex)
        {
            return Result<GetDetailOrderByIdResponse>.Failure($"Loi khi lay chi tiet don hang: {ex.Message}");
        }
    }
}
