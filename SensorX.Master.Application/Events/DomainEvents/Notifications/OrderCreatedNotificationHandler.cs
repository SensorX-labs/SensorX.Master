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
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.Events;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Events.DomainEvents.Notifications;

public class OrderCreatedNotificationHandler(
    INotificationRepository notificationRepository,
    IRealtimeNotificationService realtimeNotificationService,
    IRepository<Order> orderRepository,
    IRepository<Customer> customerRepository,
    IPublishEndpoint publishEndpoint,
    IConfiguration configuration
) : INotificationHandler<DomainEventNotification<OrderCreatedDomainEvent>>
{
    public async Task Handle(DomainEventNotification<OrderCreatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        // 1. Fetch Order details
        var order = await orderRepository.GetByIdAsync(new SensorX.Master.Domain.StrongIDs.OrderId(domainEvent.OrderId), cancellationToken);
        if (order == null) return;

        // 2. Fetch Customer
        var customer = await customerRepository.GetByIdAsync(order.CustomerId, cancellationToken);
        if (customer == null) return;

        var accountId = customer.AccountId.Value;
        var emailStr = customer.Email.Value;
        var orderCode = order.Code.Value;

        // 3. Save to Database
        var notifEntity = NotificationEntity.CreateForUser(
            userId: accountId,
            title: "Đơn hàng mới đã được khởi tạo",
            content: $"Đơn hàng {orderCode} của bạn đã được khởi tạo thành công.",
            type: "Order",
            targetUrl: $"/transactions/orders/{domainEvent.OrderId}"
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

        // 5. Build HTML Email
        var frontendUrl = (configuration["FrontendUrl"] ?? "http://localhost:3000").TrimEnd('/');
        var htmlBuilder = new StringBuilder();
        htmlBuilder.Append($"<h2>Xác nhận đơn hàng: {orderCode}</h2>");
        htmlBuilder.Append($"<p>Chào Quý khách hàng <strong>{customer.CompanyName}</strong>,</p>");
        htmlBuilder.Append("<p>Đơn hàng của quý khách đã được tạo thành công trên hệ thống. Dưới đây là thông tin chi tiết:</p>");
        htmlBuilder.Append("<table border='1' cellpadding='8' style='border-collapse: collapse;'>");
        htmlBuilder.Append("<tr style='background-color: #f2f2f2;'><th>Sản phẩm</th><th>Số lượng</th><th>Đơn giá (VND)</th><th>Thành tiền (VND)</th></tr>");

        foreach (var item in order.Items)
        {
            htmlBuilder.Append("<tr>");
            htmlBuilder.Append($"<td>{item.ProductName}</td>");
            htmlBuilder.Append($"<td align='center'>{item.Quantity.Value}</td>");
            htmlBuilder.Append($"<td align='right'>{item.UnitPrice.Amount:N0}</td>");
            htmlBuilder.Append($"<td align='right'>{item.GetTotalLineAmount().Amount:N0}</td>");
            htmlBuilder.Append("</tr>");
        }

        htmlBuilder.Append("</table>");
        htmlBuilder.Append($"<p>Tổng tiền thanh toán (đã có VAT): <strong>{order.GetGrandTotal().Amount:N0} VND</strong></p>");
        htmlBuilder.Append($"<p>Vui lòng tiến hành thanh toán theo hướng dẫn trên website hoặc liên hệ bộ phận CSKH để được hỗ trợ.</p>");
        htmlBuilder.Append($"<p><a href='{frontendUrl}/transactions/orders/{domainEvent.OrderId}'>Nhấp vào đây để xem chi tiết đơn hàng</a></p>");
        htmlBuilder.Append("<p>Cảm ơn quý khách đã tin tưởng chọn SensorX!<br/>Trân trọng,</p>");

        // 6. Publish Email to RabbitMQ
        await publishEndpoint.Publish(new SendEmailCommand
        {
            To = emailStr,
            ToName = customer.CompanyName,
            Subject = $"[SensorX] Xác nhận đơn hàng thành công {orderCode}",
            HtmlBody = htmlBuilder.ToString()
        }, cancellationToken);
    }
}
