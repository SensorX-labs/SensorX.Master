using System.Text;
using SensorX.Master.Application.Queries.Quotes.GetDetailQuoteById;
using SensorX.Master.Application.Queries.Quotes.GetPageListQuote;
using SensorX.Master.Application.Queries.Quotes.GetQuoteStats;

namespace SensorX.Master.Infrastructure.Services.Telegram;

public partial class TelegramBotBackgroundService
{
    private static string FormatHelpMessage() =>
        """
        *SensorX Telegram Bot*

        Tôi có thể hỗ trợ các tác vụ báo giá sau:

        • `Báo giá chờ duyệt`
        • `Báo giá đã duyệt`
        • `Chi tiết báo giá QT-2024-001`
        • `Duyệt báo giá QT-2024-001`
        • `Từ chối báo giá QT-2024-001 vì sai đơn giá`

        Lệnh hữu ích:
        • `/help` hoặc `/menu`: xem hướng dẫn này

        Gợi ý:
        • Bạn có thể nhắn tự nhiên, không cần nhớ tên skill nội bộ.
        • Khi cần thao tác với 1 báo giá, hãy gửi kèm mã báo giá.
        """;

    private static string FormatQuoteList(List<GetPageListQuoteResponse> items, string title)
    {
        var header = $"📋 *{title}*";
        if (items.Count == 0) return $"{header}\n\n_Không có dữ liệu._";

        var sb = new StringBuilder();
        sb.AppendLine(header)
          .AppendLine($"_Hiển thị {items.Count} bản ghi gần nhất_")
          .AppendLine();

        for (int i = 0; i < items.Count; i++)
        {
            var q = items[i];
            sb.AppendLine($"*{i + 1}. {EscapeMarkdown(q.Code)}*")
              .AppendLine($"   {QuoteEmoji(q.Status.ToString())} `{q.Status}`")
              .AppendLine($"   👤 {EscapeMarkdown(q.CompanyName)}")
              .AppendLine($"   💰 `{q.GrandTotal:N0} VND` | 📦 {q.ItemCount} SP")
              .AppendLine($"   📅 {q.CreatedAt:dd/MM/yyyy HH:mm}")
              .AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    private static string FormatQuoteDetail(GetDetailQuoteByIdResponse quote)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"📄 *Chi Tiết Báo Giá {EscapeMarkdown(quote.Code)}*")
          .AppendLine($"   {QuoteEmoji(quote.Status.ToString())} `{quote.Status}`")
          .AppendLine($"   👤 {EscapeMarkdown(quote.Customer.CompanyName)}")
          .AppendLine($"   📧 `{EscapeMarkdown(quote.Customer.Email)}`")
          .AppendLine($"   📞 `{EscapeMarkdown(quote.Customer.Phone)}`")
          .AppendLine($"   👨‍💼 {EscapeMarkdown(quote.Sender.Name)}")
          .AppendLine($"   💰 Tạm tính: `{quote.Subtotal:N0} VND`")
          .AppendLine($"   🧾 Thuế: `{quote.TotalTax:N0} VND`")
          .AppendLine($"   🏁 Tổng cộng: `{quote.GrandTotal:N0} VND`");

        if (quote.QuoteDate.HasValue)
            sb.AppendLine($"   📅 Hiệu lực đến: `{quote.QuoteDate.Value:dd/MM/yyyy HH:mm}`");

        if (!string.IsNullOrWhiteSpace(quote.Note))
            sb.AppendLine($"   📝 Ghi chú: _{EscapeMarkdown(quote.Note)}_");

        if (!string.IsNullOrWhiteSpace(quote.ReasonReject))
            sb.AppendLine($"   ❌ Lý do từ chối: _{EscapeMarkdown(quote.ReasonReject)}_");

        sb.AppendLine()
          .AppendLine("*Sản phẩm:*");

        foreach (var item in quote.Items.Take(5))
        {
            sb.AppendLine($"• `{EscapeMarkdown(item.ProductCode)}` x{item.Quantity} - `{item.TotalLineAmount:N0} VND`");
        }

        if (quote.Items.Count > 5)
            sb.AppendLine($"_Còn {quote.Items.Count - 5} sản phẩm khác..._");

        return sb.ToString().TrimEnd();
    }

    private static string EscapeMarkdown(string text) =>
        text.Replace("_", "\\_")
            .Replace("*", "\\*")
            .Replace("`", "\\`")
            .Replace("[", "\\[");

    private static string QuoteEmoji(string s) => s.ToLower() switch
    {
        var x when x.Contains("draft") => "📝",
        var x when x.Contains("pending") || x.Contains("submitted") => "⏳",
        var x when x.Contains("approved") => "✅",
        var x when x.Contains("accepted") => "🎉",
        var x when x.Contains("rejected") || x.Contains("cancel") => "❌",
        _ => "📄"
    };
}
