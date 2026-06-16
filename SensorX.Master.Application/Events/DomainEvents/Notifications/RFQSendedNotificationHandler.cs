using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Domain.Common;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;

namespace SensorX.Master.Application.Events.DomainEvents.Notifications;

public class RFQSendedNotificationHandler(
    INotificationRepository notificationRepository,
    IRealtimeNotificationService realtimeNotificationService
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
            targetUrl: $"/sales/RFQ/{domainEvent.RfqId.Value}"
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
    }
}
