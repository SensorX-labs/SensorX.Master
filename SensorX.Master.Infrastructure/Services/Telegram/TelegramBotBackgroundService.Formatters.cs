using System.Text;
using SensorX.Master.Application.Queries.Quotes.GetPageListQuote;

namespace SensorX.Master.Infrastructure.Services.Telegram;

public partial class TelegramBotBackgroundService
{
    private static string FormatQuoteList(List<GetPageListQuoteResponse> items, string title)
    {
        var header = $"📋 *{title}*";
        if (items.Count == 0) return $"{header}\n\n_Không có dữ liệu._";
        var sb = new StringBuilder();
        sb.AppendLine(header).AppendLine($"_Hiển thị {items.Count} bản ghi gần nhất_").AppendLine();
        for (int i = 0; i < items.Count; i++)
        {
            var q = items[i];
            sb.AppendLine($"*{i + 1}. {q.Code}*")
              .AppendLine($"   {QuoteEmoji(q.Status.ToString())} `{q.Status}`")
              .AppendLine($"   👤 {q.CompanyName}")
              .AppendLine($"   💰 `{q.GrandTotal:N0} VNĐ` | 📦 {q.ItemCount} SP")
              .AppendLine($"   📅 {q.CreatedAt:dd/MM/yyyy HH:mm}")
              .AppendLine();
        }
        return sb.ToString().TrimEnd();
    }

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
