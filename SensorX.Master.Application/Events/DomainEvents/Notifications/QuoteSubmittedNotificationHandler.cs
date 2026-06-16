using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Domain.Common;
using SensorX.Master.Domain.SeedWork;

using QuoteEntity = SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate.Quote;

namespace SensorX.Master.Application.Events.DomainEvents.Notifications;

public class QuoteSubmittedNotificationHandler(
    INotificationRepository notificationRepository,
    IRealtimeNotificationService realtimeNotificationService,
    IRepository<QuoteEntity> quoteRepository
) : INotificationHandler<DomainEventNotification<SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate.QuoteSubmittedForApprovalEvent>>
{
    public async Task Handle(DomainEventNotification<SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate.QuoteSubmittedForApprovalEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        // Fetch Quote to get Code
        var quote = await quoteRepository.GetByIdAsync(domainEvent.QuoteId, cancellationToken);
        var quoteCode = quote?.Code?.Value ?? "N/A";

        // 1. Save to Database
        var notifEntity = NotificationEntity.CreateForRole(
            role: "Manager",
            title: "Báo giá chờ phê duyệt",
            content: $"Báo giá {quoteCode} đã được gửi lên và đang chờ phê duyệt.",
            type: "Quote",
            targetUrl: $"/sales/quotations/{domainEvent.QuoteId.Value}"
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
