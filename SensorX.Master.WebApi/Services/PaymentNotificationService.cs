using Microsoft.AspNetCore.SignalR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.WebApi.Hubs;

namespace SensorX.Master.WebApi.Services;

public sealed class PaymentNotificationService(IHubContext<PaymentHub> paymentHubContext) : IPaymentNotificationService
{
    public async Task NotifyPaymentStatusChangedAsync(string orderId, string paymentStatus, decimal paymentAmount, CancellationToken cancellationToken)
    {
        var groupName = $"order_{orderId}";
        await paymentHubContext.Clients.Group(groupName)
            .SendAsync("PaymentStatusChanged", orderId, paymentStatus, paymentAmount, cancellationToken);
    }
}