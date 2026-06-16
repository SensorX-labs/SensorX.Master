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

public class RFQForceAssignedNotificationHandler(
    INotificationRepository notificationRepository,
    IRealtimeNotificationService realtimeNotificationService,
    IRepository<SaleStaff> staffRepository,
    IPublishEndpoint publishEndpoint,
    IConfiguration configuration
) : INotificationHandler<DomainEventNotification<RFQForceAssignedEvent>>
{
    public async Task Handle(DomainEventNotification<RFQForceAssignedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        // Fetch new SaleStaff
        var staff = await staffRepository.GetByIdAsync(domainEvent.NewStaffId, cancellationToken);
        if (staff == null) return;

        var accountId = staff.AccountId.Value;
        var emailStr = staff.Email.Value;

        // 1. Save to Database
        var notifEntity = NotificationEntity.CreateForUser(
            userId: accountId,
            title: "RFQ được chỉ định xử lý",
            content: $"Bạn đã được Quản lý chỉ định xử lý RFQ {domainEvent.Code.Value}.",
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
            Subject = $"[SensorX] Chỉ định xử lý RFQ {domainEvent.Code.Value}",
            HtmlBody = $"<h3>RFQ được chỉ định bởi Quản lý</h3><p>Chào <strong>{staff.Name}</strong>,</p><p>Bạn đã được Quản lý chỉ định trực tiếp xử lý RFQ: <strong>{domainEvent.Code.Value}</strong>.</p><p><a href='{frontendUrl}/rfq/{domainEvent.RfqId.Value}'>Nhấp vào đây để lập báo giá ngay</a></p>"
        }, cancellationToken);
    }
}
