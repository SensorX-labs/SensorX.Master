using MediatR;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.Events;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.UseCases.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler(
    IRepository<Order> orderRepository,
    IMediator mediator
) : IRequestHandler<CreateOrderCommand, Guid>
{
    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
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

        await orderRepository.AddAsync(order, cancellationToken);

        // Publish domain event via MediatR
        await mediator.Publish(new OrderCreatedDomainEvent(
            order.Id.Value,
            code.Value,
            customerInfo.RecipientName,
            customerInfo.RecipientPhone.Value,
            customerInfo.Address,
            customerInfo.CompanyName,
            customerInfo.TaxCode
        ), cancellationToken);

        return order.Id.Value;
    }
}
