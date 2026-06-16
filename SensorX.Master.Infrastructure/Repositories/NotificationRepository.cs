using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Domain.Common;
using SensorX.Master.Infrastructure.Persistences;

namespace SensorX.Master.Infrastructure.Repositories;

public class NotificationRepository(AppDbContext dbContext) : INotificationRepository
{
    public async Task AddAsync(NotificationEntity notification, CancellationToken ct)
    {
        await dbContext.Set<NotificationEntity>().AddAsync(notification, ct);
    }

    public async Task<List<NotificationEntity>> GetByUserIdAsync(Guid userId, string? role, int skip, int take, CancellationToken ct)
    {
        return await dbContext.Set<NotificationEntity>()
            .Where(n => n.UserId == userId || (role != null && n.Role == role))
            .OrderByDescending(n => n.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, string? role, CancellationToken ct)
    {
        return await dbContext.Set<NotificationEntity>()
            .CountAsync(n => (n.UserId == userId || (role != null && n.Role == role)) && !n.IsRead, ct);
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken ct)
    {
        var notification = await dbContext.Set<NotificationEntity>()
            .FirstOrDefaultAsync(n => n.Id == notificationId && (n.UserId == userId || n.Role != null), ct);
        
        if (notification != null)
        {
            notification.MarkAsRead();
        }
    }

    public async Task MarkAllAsReadAsync(Guid userId, string? role, CancellationToken ct)
    {
        var unread = await dbContext.Set<NotificationEntity>()
            .Where(n => (n.UserId == userId || (role != null && n.Role == role)) && !n.IsRead)
            .ToListAsync(ct);

        foreach (var item in unread)
        {
            item.MarkAsRead();
        }
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await dbContext.SaveChangesAsync(ct);
    }
}
