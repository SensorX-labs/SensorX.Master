namespace SensorX.Master.Application.Queries.Orders.GetOrderPaymentStatus;

public record GetOrderPaymentStatusResponse(
    Guid OrderId,
    bool IsPaid,
    string PaymentStatus
);
