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

public class RFQAssignedNotificationHandler(
    INotificationRepository notificationRepository,
    IRealtimeNotificationService realtimeNotificationService,
    IRepository<SaleStaff> staffRepository
) : INotificationHandler<DomainEventNotification<RFQAssignedEvent>>
{
    public async Task Handle(DomainEventNotification<RFQAssignedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        
        // Fetch SaleStaff to get AccountId and Email
        var staff = await staffRepository.GetByIdAsync(domainEvent.StaffId, cancellationToken);
        if (staff == null) return;

        var accountId = staff.AccountId.Value;

        // 1. Save to Database
        var notifEntity = NotificationEntity.CreateForUser(
            userId: accountId,
            title: "Yêu cầu RFQ được phân bổ",
            content: $"Bạn đã được chỉ định xử lý RFQ mới {domainEvent.Code.Value}.",
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
        await realtimeNotificationService.SendToUserAsync(accountId, payload, cancellationToken);
    }
}
