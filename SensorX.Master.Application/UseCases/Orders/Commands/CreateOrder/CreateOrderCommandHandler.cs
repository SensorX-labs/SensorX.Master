using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Services;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.PaymentAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.UseCases.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler(
    IRepository<Order> orderRepository,
    IRepository<Payment> paymentRepository,
    IInventoryAvailabilityService inventoryAvailabilityService,
    ISepayQRBuilder sepayQRBuilder,
    IUnitOfWork unitOfWork
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
        var deliveryInfo = DeliveryInfo.Create(
            request.CustomerInfo.RecipientName,
            request.CustomerInfo.RecipientPhone,
            request.CustomerInfo.ShippingAddress,
            request.CustomerInfo.CompanyName,
            Email.From(request.CustomerInfo.Email),
            request.CustomerInfo.TaxCode
        );
        var senderInfo = new SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate.SenderInfo
        {
            Name = request.SenderInfo.SenderName,
            Email = Email.From(request.SenderInfo.SenderEmail)
        };

        var order = new Order(
            new OrderId(Guid.NewGuid()),
            quoteId,
            code,
            customerId,
            deliveryInfo,
            senderInfo,
            OrderStatus.PendingPayment,
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

        var paymentType = await inventoryAvailabilityService.DeterminePaymentTypeAsync(order.Items, cancellationToken);
        var payment = new Payment(
            PaymentId.New(),
            order.Id,
            order.GetGrandTotal(),
            PaymentMethod.BankTransfer,
            PaymentStatus.Pending,
            paymentType);

        payment.SetPaymentType(paymentType);
        payment.SetQRUrls(sepayQRBuilder.BuildQRUrls(payment, order));

        order.RaiseCreatedDomainEvent();
        await orderRepository.AddAsync(order, cancellationToken);
        await paymentRepository.AddAsync(payment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return order.Id.Value;
    }
}
