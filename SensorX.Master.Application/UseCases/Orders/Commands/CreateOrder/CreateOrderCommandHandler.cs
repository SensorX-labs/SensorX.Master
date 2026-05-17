using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;
using MediatR;

namespace SensorX.Master.Application.UseCases.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler(
    IRepository<Order> orderRepository
) : IRequestHandler<CreateOrderCommand, Guid>
{
    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0)
        {
            throw new InvalidOperationException("Order must have at least one item.");
        }

        var quoteId = new QuoteId(request.QuoteId);
        var code = Code.From(request.Code);
        var customerId = new CustomerId(request.CustomerId);
        var customerInfo = new CustomerInfo(
            request.CustomerInfo.RecipientName,
            Phone.From(request.CustomerInfo.RecipientPhone),
            request.CustomerInfo.CompanyName,
            Email.From(request.CustomerInfo.Email),
            request.CustomerInfo.Address,
            request.CustomerInfo.TaxCode
        );
        var senderInfo = new SenderInfo
        {
            Name = request.SenderInfo.SenderName,
            Email = Email.From(request.SenderInfo.SenderEmail)
        };

        var order = new Order(
            new OrderId(Guid.NewGuid()),
            quoteId,
            code,
            customerId,
            customerInfo,
            senderInfo,
            OrderStatus.Processing,
            request.OrderDate
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

        await orderRepository.AddAsync(order, cancellationToken);

        return order.Id.Value;
    }
}
