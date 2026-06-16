using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Events.IntegrationEvents;
using SensorX.Master.Domain.Common;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;

namespace SensorX.Master.Application.Events.DomainEvents.Notifications;

public class RFQSendedNotificationHandler(
    INotificationRepository notificationRepository,
    IRealtimeNotificationService realtimeNotificationService,
    IPublishEndpoint publishEndpoint,
    IConfiguration configuration
) : INotificationHandler<DomainEventNotification<RFQSendedEvent>>
{
    public async Task Handle(DomainEventNotification<RFQSendedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        
        // 1. Save to Database
        var notifEntity = NotificationEntity.CreateForRole(
            role: "Manager",
            title: "Yêu cầu RFQ mới",
            content: $"RFQ mới {domainEvent.Code.Value} đã được gửi lên hệ thống.",
            type: "RFQ",
            targetUrl: $"/rfq/{domainEvent.RfqId.Value}"
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
        await realtimeNotificationService.SendToRoleAsync("Manager", payload, cancellationToken);

        // 3. Send Email to Managers
        var frontendUrl = (configuration["FrontendUrl"] ?? "http://localhost:3000").TrimEnd('/');
        await publishEndpoint.Publish(new SendEmailCommand
        {
            Role = "Manager",
            Subject = $"[SensorX] Yêu cầu báo giá (RFQ) mới {domainEvent.Code.Value}",
            HtmlBody = $"<h3>Yêu cầu báo giá mới</h3><p>Mã RFQ: <strong>{domainEvent.Code.Value}</strong> đã được gửi lên hệ thống và đang chờ xử lý.</p><p><a href='{frontendUrl}/rfq/{domainEvent.RfqId.Value}'>Nhấp vào đây để xem chi tiết</a></p>"
        }, cancellationToken);
    }
}
