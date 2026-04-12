namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.PaymentAggregate;

public enum PaymentStatus
{
    Pending, // Đang chờ - Dùng khi sinh QR 
    Completed, // Thành công - Tiền đã vào túi 
    Failed // Thất bại
}
