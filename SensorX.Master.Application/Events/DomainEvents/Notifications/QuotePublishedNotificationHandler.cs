using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Application.Events.IntegrationEvents;
using SensorX.Master.Domain.Common;
using SensorX.Master.Domain.SeedWork;

using QuoteEntity = SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate.Quote;

namespace SensorX.Master.Application.Events.DomainEvents.Notifications;

public class QuotePublishedNotificationHandler(
    INotificationRepository notificationRepository,
    IRealtimeNotificationService realtimeNotificationService,
    IRepository<QuoteEntity> quoteRepository,
    IRepository<Customer> customerRepository,
    IPublishEndpoint publishEndpoint,
    IConfiguration configuration
) : INotificationHandler<DomainEventNotification<SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate.PublishQuoteEvent>>
{
    public async Task Handle(DomainEventNotification<SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate.PublishQuoteEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        // 1. Fetch Quote
        var quote = await quoteRepository.GetByIdAsync(domainEvent.QuoteId, cancellationToken);
        if (quote == null) return;

        // 2. Fetch Customer
        var customer = await customerRepository.GetByIdAsync(quote.CustomerId, cancellationToken);
        if (customer == null) return;

        var accountId = customer.AccountId.Value;
        var emailStr = customer.Email.Value;
        var quoteCode = quote.Code.Value;

        // 3. Save to Database
        var notifEntity = NotificationEntity.CreateForUser(
            userId: accountId,
            title: "Báo giá mới đã sẵn sàng",
            content: $"Báo giá {quoteCode} cho yêu cầu RFQ của bạn đã được lập.",
            type: "Quote",
            targetUrl: $"/transactions/quotations/{domainEvent.QuoteId.Value}"
        );
        await notificationRepository.AddAsync(notifEntity, cancellationToken);
        await notificationRepository.SaveChangesAsync(cancellationToken);

        // 4. Push SignalR
        var payload = new
        {
            id = notifEntity.Id,
            title = notifEntity.Title,
            content = notifEntity.Content,
            type = notifEntity.Type,
            targetUrl = notifEntity.TargetUrl,
            isRead = notifEntity.IsRead,
            createdAt = notifEntity.CreatedAt
        };
        await realtimeNotificationService.SendToUserAsync(accountId, payload, cancellationToken);

        // 5. Build HTML Body for email
        var frontendUrl = (configuration["FrontendUrl"] ?? "http://localhost:3000").TrimEnd('/');
        var htmlBuilder = new StringBuilder();
        htmlBuilder.Append($"<h2>Thông báo Báo giá mới: {quoteCode}</h2>");
        htmlBuilder.Append($"<p>Chào Quý khách hàng <strong>{customer.CompanyName}</strong>,</p>");
        htmlBuilder.Append("<p>Chúng tôi xin gửi thông tin chi tiết báo giá mà quý khách đã yêu cầu:</p>");
        htmlBuilder.Append("<table border='1' cellpadding='8' style='border-collapse: collapse;'>");
        htmlBuilder.Append("<tr style='background-color: #f2f2f2;'><th>Sản phẩm</th><th>Số lượng</th><th>Đơn giá (VND)</th><th>Thành tiền (VND)</th></tr>");
        
        foreach (var item in quote.LineItems)
        {
            htmlBuilder.Append("<tr>");
            htmlBuilder.Append($"<td>{item.ProductName}</td>");
            htmlBuilder.Append($"<td align='center'>{item.Quantity.Value}</td>");
            htmlBuilder.Append($"<td align='right'>{item.UnitPrice.Amount:N0}</td>");
            htmlBuilder.Append($"<td align='right'>{item.GetTotalLineAmount().Amount:N0}</td>");
            htmlBuilder.Append("</tr>");
        }

        htmlBuilder.Append("</table>");
        htmlBuilder.Append($"<p>Tổng cộng (bao gồm VAT): <strong>{quote.GetGrandTotal().Amount:N0} VND</strong></p>");
        htmlBuilder.Append($"<p><a href='{frontendUrl}/transactions/quotations/{domainEvent.QuoteId.Value}'>Nhấp vào đây để xem chi tiết, Phản hồi hoặc Chấp nhận báo giá</a></p>");
        htmlBuilder.Append("<p>Trân trọng,<br/>Đội ngũ SensorX</p>");

        // 6. Send Email to Customer
        await publishEndpoint.Publish(new SendEmailCommand
        {
            To = emailStr,
            ToName = customer.CompanyName,
            Subject = $"[SensorX] Báo giá chính thức {quoteCode} đã được lập",
            HtmlBody = htmlBuilder.ToString()
        }, cancellationToken);
    }
}
