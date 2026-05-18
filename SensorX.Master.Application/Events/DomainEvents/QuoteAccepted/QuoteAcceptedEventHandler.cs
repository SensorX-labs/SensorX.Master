using MediatR;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Application.UseCases.Orders.Commands.CreateOrder;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.Events;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.Events.DomainEvents.QuoteAccepted;

public class QuoteAcceptedEventHandler(
    IRepository<Quote> _quoteRepository,
    IMediator _mediator
) : INotificationHandler<DomainEventNotification<QuoteAcceptedEvent>>
{
    public async Task Handle(
        DomainEventNotification<QuoteAcceptedEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        
        var quoteId = new QuoteId(domainEvent.QuoteId);
        var quote = await _quoteRepository.GetByIdAsync(quoteId, cancellationToken);
        
        if (quote != null)
        {
            var customerInfo = new CustomerInfoDto(
                RecipientName: quote.CustomerInfo.RecipientName,
                RecipientPhone: quote.CustomerInfo.RecipientPhone.Value,
                ShippingAddress: quote.CustomerInfo.ShippingAddress,
                CompanyName: quote.CustomerInfo.CompanyName,
                Email: quote.CustomerInfo.Email.Value,
                Address: quote.CustomerInfo.Address,
                TaxCode: quote.CustomerInfo.TaxCode
            );

            var senderInfo = new SenderInfoDto(
                SenderName: "SensorX System",
                SenderEmail: "admin@sensorx.com"
            );

            var createOrderCommand = new CreateOrderCommand(
                QuoteId: quote.Id.Value,
                Code: Code.Create("ORD").Value,
                CustomerId: quote.CustomerId.Value,
                CustomerInfo: customerInfo,
                SenderInfo: senderInfo,
                OrderDate: DateTimeOffset.UtcNow,
                Items: quote.LineItems.Select(item => new OrderItemDto(
                    ProductId: item.ProductId.Value,
                    ProductCode: item.ProductCode.Value,
                    ProductName: item.ProductCode.Value,
                    Manufacturer: item.Manufacturer,
                    Unit: item.Unit,
                    Quantity: item.Quantity.Value,
                    UnitPrice: item.UnitPrice.Amount,
                    TaxRate: item.TaxRate.Value,
                    Note: null
                )).ToList()
            );

            // Sinh đơn hàng ngay lập tức khi Quote đã được chốt
            await _mediator.Send(createOrderCommand, cancellationToken);
        }
    }
}
