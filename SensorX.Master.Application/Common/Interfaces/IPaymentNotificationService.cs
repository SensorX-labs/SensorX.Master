namespace SensorX.Master.Application.Common.Interfaces;

public interface IPaymentNotificationService
{
    Task NotifyPaymentStatusChangedAsync(string orderId, string paymentStatus, decimal paymentAmount, CancellationToken cancellationToken);
}