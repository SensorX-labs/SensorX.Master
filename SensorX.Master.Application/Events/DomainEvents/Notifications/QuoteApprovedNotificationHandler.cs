using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Domain.Common;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.SeedWork;

using QuoteEntity = SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate.Quote;

namespace SensorX.Master.Application.Events.DomainEvents.Notifications;

public class QuoteApprovedNotificationHandler(
    INotificationRepository notificationRepository,
    IRealtimeNotificationService realtimeNotificationService,
    IRepository<QuoteEntity> quoteRepository,
    IRepository<SaleStaff> staffRepository
) : INotificationHandler<DomainEventNotification<QuoteApprovedEvent>>
{
    public async Task Handle(DomainEventNotification<QuoteApprovedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        // Fetch Quote to get Code
        var quote = await quoteRepository.GetByIdAsync(domainEvent.QuoteId, cancellationToken);
        var quoteCode = quote?.Code?.Value ?? "N/A";

        // Fetch SaleStaff to get AccountId
        var staff = await staffRepository.GetByIdAsync(domainEvent.StaffId, cancellationToken);
        if (staff == null) return;

        var accountId = staff.AccountId.Value;

        // 1. Save to Database
        var notifEntity = NotificationEntity.CreateForUser(
            userId: accountId,
            title: "Báo giá được phê duyệt",
            content: $"Báo giá {quoteCode} đã được Quản lý phê duyệt.",
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
        await realtimeNotificationService.SendToUserAsync(accountId, payload, cancellationToken);
    }
}
