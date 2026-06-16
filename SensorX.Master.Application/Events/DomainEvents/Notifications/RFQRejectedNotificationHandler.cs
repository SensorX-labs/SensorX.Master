using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Domain.Common;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Events.DomainEvents.Notifications;

public class RFQRejectedNotificationHandler(
    INotificationRepository notificationRepository,
    IRealtimeNotificationService realtimeNotificationService,
    IRepository<SaleStaff> staffRepository
) : INotificationHandler<DomainEventNotification<RFQRejectedEvent>>
{
    public async Task Handle(DomainEventNotification<RFQRejectedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        // Fetch staff name who rejected
        var staff = await staffRepository.GetByIdAsync(domainEvent.StaffId, cancellationToken);
        var staffName = staff?.Name ?? "Nhân viên";

        // 1. Save to Database
        var notifEntity = NotificationEntity.CreateForRole(
            role: "Manager",
            title: "Yêu cầu RFQ bị từ chối",
            content: $"RFQ {domainEvent.Code.Value} đã bị từ chối bởi {staffName}.",
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
    }
}
