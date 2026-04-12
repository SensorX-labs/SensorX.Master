namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;

public enum OrderStatus
{
    PendingPayment, // Chờ thanh toán
    Processing, // Đang chuẩn bị đơn hàng
    Dispatched, // Đã xuất kho
    Cancelled // Đơn bị hủy
}