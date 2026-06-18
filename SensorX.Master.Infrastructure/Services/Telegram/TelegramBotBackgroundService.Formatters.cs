using System.Text;
using SensorX.Master.Application.Queries.Orders.GetDetailOrderById;
using SensorX.Master.Application.Queries.Orders.GetPageListOrder;
using SensorX.Master.Application.Queries.Quotes.GetDetailQuoteById;
using SensorX.Master.Application.Queries.Quotes.GetPageListQuote;

namespace SensorX.Master.Infrastructure.Services.Telegram;

public partial class TelegramBotBackgroundService
{
    private static string FormatHelpMessage() =>
        """
        *SensorX Telegram Bot*

        *Báo giá:*
        - `Báo giá chờ duyệt`
        - `Báo giá đã duyệt`
        - `Chi tiết báo giá QT-2024-001`
        - `Duyệt báo giá QT-2024-001`
        - `Từ chối báo giá QT-2024-001 vì sai đơn giá`

        *Đơn hàng:*
        - `Danh sách đơn hàng`
        - `Chi tiết đơn hàng ORD-2024-001`

        Lệnh hữu ích:
        - `/help` hoặc `/menu`: xem hướng dẫn này

        Gợi ý:
        - Bạn có thể nhắn tự nhiên, không cần nhớ cú pháp chính xác.
        - Khi hỏi về 1 bản ghi cụ thể, gửi kèm mã (VD: QT-2024-001, ORD-2024-001).
        """;

    private static string FormatQuoteList(List<GetPageListQuoteResponse> items, string title)
    {
        var header = $"*{title}*";
        if (items.Count == 0) return $"{header}\n\n_Không có dữ liệu._";

        var sb = new StringBuilder();
        sb.AppendLine(header)
          .AppendLine($"_Hiển thị {items.Count} bản ghi gần nhất_")
          .AppendLine();

        for (int i = 0; i < items.Count; i++)
        {
            var quote = items[i];
            sb.AppendLine($"*{i + 1}. {EscapeMarkdown(quote.Code)}*")
              .AppendLine($"   {QuoteEmoji(quote.Status.ToString())} `{quote.Status}`")
              .AppendLine($"   Công ty: {EscapeMarkdown(quote.CompanyName)}")
              .AppendLine($"   Tổng tiền: `{quote.GrandTotal:N0} VND` | Sản phẩm: {quote.ItemCount}")
              .AppendLine($"   Tạo lúc: {quote.CreatedAt:dd/MM/yyyy HH:mm}")
              .AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    private static string FormatQuoteDetail(GetDetailQuoteByIdResponse quote)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"*Chi Tiết Báo Giá {EscapeMarkdown(quote.Code)}*")
          .AppendLine($"   {QuoteEmoji(quote.Status.ToString())} `{quote.Status}`")
          .AppendLine($"   Công ty: {EscapeMarkdown(quote.Customer.CompanyName)}")
          .AppendLine($"   Email: `{EscapeMarkdown(quote.Customer.Email)}`")
          .AppendLine($"   Điện thoại: `{EscapeMarkdown(quote.Customer.Phone)}`")
          .AppendLine($"   Nhân viên: {EscapeMarkdown(quote.Sender.Name)}")
          .AppendLine($"   Tạm tính: `{quote.Subtotal:N0} VND`")
          .AppendLine($"   Thuế: `{quote.TotalTax:N0} VND`")
          .AppendLine($"   Tổng cộng: `{quote.GrandTotal:N0} VND`");

        if (quote.QuoteDate.HasValue)
            sb.AppendLine($"   Hiệu lực đến: `{quote.QuoteDate.Value:dd/MM/yyyy HH:mm}`");

        if (!string.IsNullOrWhiteSpace(quote.Note))
            sb.AppendLine($"   Ghi chú: _{EscapeMarkdown(quote.Note)}_");

        if (!string.IsNullOrWhiteSpace(quote.ReasonReject))
            sb.AppendLine($"   Lý do từ chối: _{EscapeMarkdown(quote.ReasonReject)}_");

        sb.AppendLine()
          .AppendLine("*Sản phẩm:*");

        foreach (var item in quote.Items.Take(5))
        {
            sb.AppendLine($"- `{EscapeMarkdown(item.ProductCode)}` x{item.Quantity} - `{item.TotalLineAmount:N0} VND`");
        }

        if (quote.Items.Count > 5)
            sb.AppendLine($"_Còn {quote.Items.Count - 5} sản phẩm khác..._");

        return sb.ToString().TrimEnd();
    }

    private static string FormatOrderList(List<GetPageListOrderResponse> items, string title)
    {
        var header = $"*{title}*";
        if (items.Count == 0) return $"{header}\n\n_Không có dữ liệu._";

        var sb = new StringBuilder();
        sb.AppendLine(header)
          .AppendLine($"_Hiển thị {items.Count} bản ghi gần nhất_")
          .AppendLine();

        for (int i = 0; i < items.Count; i++)
        {
            var order = items[i];
            sb.AppendLine($"*{i + 1}. {EscapeMarkdown(order.Code)}*")
              .AppendLine($"   {OrderEmoji(order.Status)} `{order.Status}`")
              .AppendLine($"   Công ty: {EscapeMarkdown(order.CompanyName)}")
              .AppendLine($"   Tổng tiền: `{order.GrandTotal:N0} VND` | Sản phẩm: {order.ItemCount}")
              .AppendLine($"   Ngày đặt: {order.OrderDate:dd/MM/yyyy HH:mm}")
              .AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    private static string FormatOrderDetail(GetDetailOrderByIdResponse order)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"*Chi Tiết Đơn Hàng {EscapeMarkdown(order.Code)}*")
          .AppendLine($"   {OrderEmoji(order.Status)} `{order.Status}`")
          .AppendLine($"   Công ty: {EscapeMarkdown(order.CompanyName)}")
          .AppendLine($"   Người nhận: {EscapeMarkdown(order.RecipientName)} - `{EscapeMarkdown(order.RecipientPhone)}`")
          .AppendLine($"   Email: `{EscapeMarkdown(order.Email)}`")
          .AppendLine($"   Địa chỉ: {EscapeMarkdown(order.Address)}")
          .AppendLine($"   Nhân viên: {EscapeMarkdown(order.SenderName)}")
          .AppendLine($"   Tạm tính: `{order.Subtotal:N0} VND`")
          .AppendLine($"   Thuế: `{order.TotalTax:N0} VND`")
          .AppendLine($"   Tổng cộng: `{order.GrandTotal:N0} VND`")
          .AppendLine($"   Ngày đặt: `{order.OrderDate:dd/MM/yyyy HH:mm}`");

        if (!string.IsNullOrWhiteSpace(order.PaymentStatus) || !string.IsNullOrWhiteSpace(order.PaymentType))
            sb.AppendLine($"   Thanh toán: `{order.PaymentStatus ?? "N/A"}` ({order.PaymentType ?? "N/A"})");

        if (order.PaymentAmount.HasValue)
            sb.AppendLine($"   Số tiền đã thanh toán: `{order.PaymentAmount.Value:N0} VND`");

        sb.AppendLine()
          .AppendLine("*Sản phẩm:*");

        foreach (var item in order.Items.Take(5))
        {
            sb.AppendLine($"- `{EscapeMarkdown(item.ProductCode)}` x{item.Quantity} - `{item.TotalLineAmount:N0} VND`");
        }

        if (order.Items.Count > 5)
            sb.AppendLine($"_Còn {order.Items.Count - 5} sản phẩm khác..._");

        return sb.ToString().TrimEnd();
    }

    private static string EscapeMarkdown(string text) =>
        text.Replace("_", "\\_")
            .Replace("*", "\\*")
            .Replace("`", "\\`")
            .Replace("[", "\\[");

    private static string QuoteEmoji(string s) => s.ToLowerInvariant() switch
    {
        var x when x.Contains("draft") => "[nháp]",
        var x when x.Contains("pending") || x.Contains("submitted") => "[chờ duyệt]",
        var x when x.Contains("approved") => "[đã duyệt]",
        var x when x.Contains("accepted") => "[đã chấp nhận]",
        var x when x.Contains("rejected") || x.Contains("cancel") => "[từ chối]",
        _ => "[báo giá]"
    };

    private static string OrderEmoji(string s) => s.ToLowerInvariant() switch
    {
        var x when x.Contains("pending") => "[chờ xử lý]",
        var x when x.Contains("paid") || x.Contains("completed") => "[hoàn tất]",
        var x when x.Contains("shipping") || x.Contains("deliver") => "[đang giao]",
        var x when x.Contains("cancel") || x.Contains("reject") => "[đã hủy]",
        _ => "[đơn hàng]"
    };
}
