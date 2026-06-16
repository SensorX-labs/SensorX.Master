using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SensorX.Master.Domain.Common;

namespace SensorX.Master.Application.Common.Interfaces;

public interface INotificationRepository
{
    Task AddAsync(NotificationEntity notification, CancellationToken ct);
    Task<List<NotificationEntity>> GetByUserIdAsync(Guid userId, string? role, int skip, int take, CancellationToken ct);
    Task<int> GetUnreadCountAsync(Guid userId, string? role, CancellationToken ct);
    Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken ct);
    Task MarkAllAsReadAsync(Guid userId, string? role, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
