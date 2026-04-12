namespace SensorX.Master.Domain.Contexts.QuoteContext
{
    public enum QuoteStatus
    {
        Draft, //Bản thảo
        Pending, //Chờ duyệt
        Returned, //Sếp từ chối
        Approved, //Đã duyệt
        Sent, //Đã gửi
        Ordered, //Đã sinh đơn hàng
        Expired //Hết hiệu lực
    }
}