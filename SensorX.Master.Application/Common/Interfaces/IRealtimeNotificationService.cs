using System;
using System.Threading;
using System.Threading.Tasks;

namespace SensorX.Master.Application.Common.Interfaces;

public interface IRealtimeNotificationService
{
    Task SendToUserAsync(Guid accountId, object payload, CancellationToken ct = default);
    Task SendToRoleAsync(string role, object payload, CancellationToken ct = default);
}
