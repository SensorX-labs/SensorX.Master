using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Common.Exceptions;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.Commands.Orders.CreateOrder;

public class CreateOrderHandler(
    IRepository<Order> orderRepository
) : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.Items == null || request.Items.Count == 0)
            {
                return Result<Guid>.Failure("Order must have at least one item.");
            }

            var deliveryInfo = DeliveryInfo.Create(
                request.CustomerInfo.RecipientName,
                request.CustomerInfo.RecipientPhone,
                request.CustomerInfo.ShippingAddress,
                request.CustomerInfo.CompanyName,
                Email.From(request.CustomerInfo.Email),
                request.CustomerInfo.TaxCode
            );

            var senderInfo = new SenderInfo
            {
                Name = request.SenderInfo.SenderName,
                Email = Email.From(request.SenderInfo.SenderEmail)
            };

            var order = new Order(
                new OrderId(Guid.NewGuid()),
                new QuoteId(request.QuoteId),
                string.IsNullOrWhiteSpace(request.Code) ? Code.Create("ORD") : Code.From(request.Code),
                new CustomerId(request.CustomerId),
                deliveryInfo,
                senderInfo,
                OrderStatus.Processing,
                request.OrderDate ?? DateTimeOffset.UtcNow
            );

            foreach (var item in request.Items)
            {
                order.AddItem(new OrderItem(
                    new OrderItemId(Guid.NewGuid()),
                    new ProductId(item.ProductId),
                    Code.From(item.ProductCode),
                    item.ProductName,
                    item.Manufacturer,
                    item.Unit,
                    new Quantity(item.Quantity),
                    Money.FromVnd(item.UnitPrice),
                    Percent.From(item.TaxRate),
                    item.Note
                ));
            }

            order.RaiseCreatedDomainEvent();
            await orderRepository.AddAsync(order, cancellationToken);

            return Result<Guid>.Success(order.Id.Value);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Failed to create order: {ex.Message}");
        }
    }
}
