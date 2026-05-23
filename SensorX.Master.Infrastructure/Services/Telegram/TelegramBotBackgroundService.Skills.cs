using MediatR;
using SensorX.Master.Application.Commands.Quotes.ApproveQuote;
using SensorX.Master.Application.Queries.Quotes.GetPageListQuote;
using SensorX.Master.Application.Queries.Quotes.GetQuoteByCode;

namespace SensorX.Master.Infrastructure.Services.Telegram;

public partial class TelegramBotBackgroundService
{
    private async Task<string> HandleQuotesAsync(IMediator mediator, string statusFilter, CancellationToken ct)
    {
        var result = await mediator.Send(new GetPageListQuoteQuery(null, null, null, null) { PageNumber = 1, PageSize = 10 }, ct);
        if (!result.IsSuccess || result.Value is null) return "Không thể truy vấn báo giá từ database.";

        var items = result.Value.Items.AsEnumerable();
        if (statusFilter == "Pending")
        {
            string[] pending = ["PendingApproval", "Pending", "Submitted"];
            items = items.Where(q => pending.Any(s => q.Status.ToString().Contains(s, StringComparison.OrdinalIgnoreCase)));
            return FormatQuoteList(items.ToList(), "Báo Giá Chờ Duyệt");
        }
        else if (statusFilter == "Approved")
        {
            items = items.Where(q => q.Status.ToString().Contains("Approved", StringComparison.OrdinalIgnoreCase));
            return FormatQuoteList(items.ToList(), "Báo Giá Đã Duyệt");
        }

        return FormatQuoteList(items.ToList(), "Danh Sách Báo Giá");
    }

    private async Task<string> HandleQuoteApprove(IMediator mediator, string quoteCode, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(quoteCode))
            return "Vui lòng cung cấp mã báo giá. Ví dụ: _Duyệt báo giá QT-2024-001_";

        // Tìm báo giá theo Code
        var search = await mediator.Send(new GetQuoteByCodeQuery(quoteCode), ct);
        if (!search.IsSuccess || search.Value is null)
            return $"Không tìm thấy báo giá với mã *{quoteCode}*";

        var quote = search.Value;

        // Thực hiện phê duyệt
        var result = await mediator.Send(new ApproveQuoteCommand(quote.Id), ct);
        if (!result.IsSuccess)
            return $"Không thể duyệt báo giá *{quoteCode}*: {result.Message}";

        return $"✅ Đã phê duyệt thành công báo giá *{quoteCode}*\n" +
               $"   👤 {quote.CompanyName}\n" +
               $"   💰 `{quote.GrandTotal:N0} VNĐ`";
    }
}
