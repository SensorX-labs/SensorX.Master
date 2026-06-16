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
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Events.DomainEvents.Notifications;

public class RFQAssignedNotificationHandler(
    INotificationRepository notificationRepository,
    IRealtimeNotificationService realtimeNotificationService,
    IRepository<SaleStaff> staffRepository,
    IPublishEndpoint publishEndpoint,
    IConfiguration configuration
) : INotificationHandler<DomainEventNotification<RFQAssignedEvent>>
{
    public async Task Handle(DomainEventNotification<RFQAssignedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        
        // Fetch SaleStaff to get AccountId and Email
        var staff = await staffRepository.GetByIdAsync(domainEvent.StaffId, cancellationToken);
        if (staff == null) return;

        var accountId = staff.AccountId.Value;
        var emailStr = staff.Email.Value;

        // 1. Save to Database
        var notifEntity = NotificationEntity.CreateForUser(
            userId: accountId,
            title: "Yêu cầu RFQ được phân bổ",
            content: $"Bạn đã được chỉ định xử lý RFQ mới {domainEvent.Code.Value}.",
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
        await realtimeNotificationService.SendToUserAsync(accountId, payload, cancellationToken);

        // 3. Send Email to SaleStaff
        var frontendUrl = (configuration["FrontendUrl"] ?? "http://localhost:3000").TrimEnd('/');
        await publishEndpoint.Publish(new SendEmailCommand
        {
            To = emailStr,
            ToName = staff.Name,
            Subject = $"[SensorX] Yêu cầu RFQ {domainEvent.Code.Value} được phân bổ cho bạn",
            HtmlBody = $"<h3>Phân bổ RFQ thành công</h3><p>Chào <strong>{staff.Name}</strong>,</p><p>Bạn đã được phân công xử lý RFQ: <strong>{domainEvent.Code.Value}</strong>.</p><p><a href='{frontendUrl}/rfq/{domainEvent.RfqId.Value}'>Nhấp vào đây để xem chi tiết và lập báo giá</a></p>"
        }, cancellationToken);
    }
}
