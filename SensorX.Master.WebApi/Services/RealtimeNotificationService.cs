using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.WebApi.Hubs;

namespace SensorX.Master.WebApi.Services;

public class RealtimeNotificationService(IHubContext<NotificationHub> notificationHubContext) : IRealtimeNotificationService
{
    public async Task SendToUserAsync(Guid accountId, object payload, CancellationToken ct = default)
    {
        var groupName = $"user_{accountId}";
        await notificationHubContext.Clients.Group(groupName)
            .SendAsync("ReceiveNotification", payload, ct);
    }

    public async Task SendToRoleAsync(string role, object payload, CancellationToken ct = default)
    {
        var groupName = $"role_{role}";
        await notificationHubContext.Clients.Group(groupName)
            .SendAsync("ReceiveNotification", payload, ct);
    }
}
