namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;

public enum InvoiceStatus
{
    Unpaid, // Chờ thanh toán
    PartiallyPaid, // Thanh toán một phần
    Paid, // Đã thanh toán
    Issued, // Đã phát hành
    Cancelled // Đã hủy
}
