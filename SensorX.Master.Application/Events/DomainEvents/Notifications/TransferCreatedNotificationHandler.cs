using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Events.IntegrationEvents;
using SensorX.Master.Domain.Common;
using SensorX.Master.Domain.Events;

namespace SensorX.Master.Application.Events.DomainEvents.Notifications;

public class TransferCreatedNotificationHandler(
    INotificationRepository notificationRepository,
    IRealtimeNotificationService realtimeNotificationService,
    IPublishEndpoint publishEndpoint,
    IConfiguration configuration
) : INotificationHandler<DomainEventNotification<TransferOrderCreatedDomainEvent>>
{
    public async Task Handle(DomainEventNotification<TransferOrderCreatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var codeStr = domainEvent.TransferOrderCode ?? "N/A";

        // 1. Save to Database
        var notifEntity = NotificationEntity.CreateForRole(
            role: "WarehouseStaff",
            title: "Yêu cầu vận chuyển mới",
            content: $"Yêu cầu vận chuyển kho {codeStr} đã được tạo.",
            type: "Warehouse",
            targetUrl: $"/warehouse/transfers/{domainEvent.TransferOrderId}"
        );
        await notificationRepository.AddAsync(notifEntity, cancellationToken);
        await notificationRepository.SaveChangesAsync(cancellationToken);

        // 2. Push SignalR
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
        await realtimeNotificationService.SendToRoleAsync("WarehouseStaff", payload, cancellationToken);

        // 3. Build HTML Body
        var frontendUrl = (configuration["FrontendUrl"] ?? "http://localhost:3000").TrimEnd('/');
        var htmlBuilder = new StringBuilder();
        htmlBuilder.Append($"<h2>Yêu cầu vận chuyển liên kho mới: {codeStr}</h2>");
        htmlBuilder.Append("<p>Thông báo tới bộ phận Kho,</p>");
        htmlBuilder.Append("<p>Một phiếu yêu cầu chuyển kho mới đã được lập trên hệ thống với thông tin chi tiết:</p>");
        htmlBuilder.Append("<table border='1' cellpadding='8' style='border-collapse: collapse;'>");
        htmlBuilder.Append("<tr style='background-color: #f2f2f2;'><th>Mã sản phẩm</th><th>Tên sản phẩm</th><th>Số lượng</th><th>Đơn vị</th></tr>");

        foreach (var item in domainEvent.Items)
        {
            htmlBuilder.Append("<tr>");
            htmlBuilder.Append($"<td>{item.ProductCode}</td>");
            htmlBuilder.Append($"<td>{item.ProductName}</td>");
            htmlBuilder.Append($"<td align='center'>{item.Quantity}</td>");
            htmlBuilder.Append($"<td>{item.Unit}</td>");
            htmlBuilder.Append("</tr>");
        }

        htmlBuilder.Append("</table>");
        htmlBuilder.Append($"<p><a href='{frontendUrl}/warehouse/transfers/{domainEvent.TransferOrderId}'>Nhấp vào đây để xem chi tiết và xử lý xuất/nhập kho</a></p>");
        htmlBuilder.Append("<p>Trân trọng,<br/>Hệ thống Quản lý Kho SensorX</p>");

        // 4. Publish Email to WarehouseStaff
        await publishEndpoint.Publish(new SendEmailCommand
        {
            Role = "WarehouseStaff",
            Subject = $"[SensorX] Phiếu chuyển kho mới {codeStr} đang chờ xử lý",
            HtmlBody = htmlBuilder.ToString()
        }, cancellationToken);
    }
}
