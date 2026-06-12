using MediatR;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Application.UseCases.Orders.Commands.CreateOrder;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;
using QuoteAggregate = SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;

namespace SensorX.Master.Application.Events.DomainEvents.Quote.CustomerRespondedQuote;

public class CreateOrderEventHandler(
    IRepository<QuoteAggregate.Quote> _quoteRepository,
    IMediator _mediator
) : INotificationHandler<DomainEventNotification<QuoteAggregate.CustomerRespondedQuoteEvent>>
{
    public async Task Handle(
        DomainEventNotification<QuoteAggregate.CustomerRespondedQuoteEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        if (domainEvent.QuoteResponse.ResponseType != QuoteAggregate.QuoteResponseStatus.Accepted) return;

        var quote = await _quoteRepository.GetByIdAsync(domainEvent.QuoteId, cancellationToken);

        if (quote is not null)
        {
            var customerInfo = new CustomerInfoDto(
                RecipientName: quote.CustomerInfo.CompanyName,
                RecipientPhone: quote.CustomerInfo.Phone.Value,
                ShippingAddress: quote.Response?.ShippingAddress ?? quote.CustomerInfo.Address,
                CompanyName: quote.CustomerInfo.CompanyName,
                Email: quote.CustomerInfo.Email.Value,
                Address: quote.CustomerInfo.Address,
                TaxCode: quote.CustomerInfo.TaxCode
            );

            var senderInfo = new SenderInfoDto(
                SenderName: quote.SenderInfo.Name,
                SenderEmail: quote.SenderInfo.Email
            );

            var createOrderCommand = new CreateOrderCommand(
                QuoteId: quote.Id.Value,
                Code: Code.Create("ORD").Value,
                CustomerId: quote.CustomerId.Value,
                CustomerInfo: customerInfo,
                SenderInfo: senderInfo,
                OrderDate: DateTimeOffset.UtcNow,
                Items: quote.LineItems.Select(item => new OrderItemDto(
                    ProductId: item.ProductId,
                    ProductCode: item.ProductCode,
                    ProductName: item.ProductName,
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
