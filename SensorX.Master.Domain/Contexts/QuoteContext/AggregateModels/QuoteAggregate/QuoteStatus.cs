namespace SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate
{
    public enum QuoteStatus
    {
        Draft, //Bản thảo
        Pending, //Chờ duyệt
        Returned, //Sếp từ chối
        Approved, //Đã duyệt
        Sent, //Đã gửi
        Ordered, //Đã sinh đơn hàng
        Cancelled //Đã hủy
    }
}
