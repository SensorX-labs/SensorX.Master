namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.PaymentAggregate;

public enum PaymentStatus
{
    Pending, // Đang chờ - Dùng khi sinh QR 
    PartiallyPaid, // Đã thanh toán một phần - Thanh toán 30%
    Completed, // Thành công - Tiền đã vào túi 
    Failed // Thất bại
}
