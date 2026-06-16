using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Domain.Common;
using SensorX.Master.Domain.Events;

namespace SensorX.Master.Application.Events.DomainEvents.Notifications;

public class TransferCreatedNotificationHandler(
    INotificationRepository notificationRepository,
    IRealtimeNotificationService realtimeNotificationService
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
            targetUrl: $"/warehouse/transfer-orders/{domainEvent.TransferOrderId}"
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
    }
}
