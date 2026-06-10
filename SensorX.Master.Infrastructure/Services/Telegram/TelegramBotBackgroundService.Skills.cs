using MediatR;
using SensorX.Master.Application.Commands.Quotes.ApproveQuote;
using SensorX.Master.Application.Commands.Quotes.RejectQuote;
using SensorX.Master.Application.Queries.Quotes.GetDetailQuoteById;
using SensorX.Master.Application.Queries.Quotes.GetPageListQuote;
using SensorX.Master.Application.Queries.Quotes.GetQuoteByCode;

namespace SensorX.Master.Infrastructure.Services.Telegram;

public partial class TelegramBotBackgroundService
{
    private async Task<string> HandleQuotesAsync(IMediator mediator, string statusFilter, CancellationToken ct)
    {
        var result = await mediator.Send(
            new GetPageListQuoteQuery(
                SearchTerm: null,
                Status: null,
                ResponseType: null,
                IsExpired: null,
                Code: null,
                CompanyName: null,
                CustomerEmail: null,
                CustomerPhone: null,
                SenderName: null,
                TotalFrom: null,
                TotalTo: null,
                QuoteDateFrom: null,
                QuoteDateTo: null,
                CreatedFrom: null,
                CreatedTo: null
            )
            {
                PageNumber = 1,
                PageSize = 10
            },
            ct
        );

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

    private async Task<string> HandleQuoteDetailAsync(IMediator mediator, string quoteCode, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(quoteCode))
            return "Vui lòng cung cấp mã báo giá. Ví dụ: _Chi tiết báo giá QT-2024-001_";

        var search = await mediator.Send(new GetQuoteByCodeQuery(quoteCode), ct);
        if (!search.IsSuccess || search.Value is null)
            return $"Không tìm thấy báo giá với mã *{quoteCode}*";

        var detail = await mediator.Send(new GetDetailQuoteByIdQuery(search.Value.Id), ct);
        if (!detail.IsSuccess || detail.Value is null)
            return $"Không lấy được chi tiết báo giá *{quoteCode}*: {detail.Message}";

        return FormatQuoteDetail(detail.Value);
    }

    private async Task<string> HandleQuoteApprove(IMediator mediator, string quoteCode, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(quoteCode))
            return "Vui lòng cung cấp mã báo giá. Ví dụ: _Duyệt báo giá QT-2024-001_";

        var search = await mediator.Send(new GetQuoteByCodeQuery(quoteCode), ct);
        if (!search.IsSuccess || search.Value is null)
            return $"Không tìm thấy báo giá với mã *{quoteCode}*";

        var quote = search.Value;

        var result = await mediator.Send(new ApproveQuoteCommand(quote.Id), ct);
        if (!result.IsSuccess)
            return $"Không thể duyệt báo giá *{quoteCode}*: {result.Message}";

        return $"Đã phê duyệt thành công báo giá *{quoteCode}*\n" +
               $"   {quote.CompanyName}\n" +
               $"   `{quote.GrandTotal:N0} VND`";
    }

    private async Task<string> HandleQuoteRejectAsync(IMediator mediator, string payload, CancellationToken ct)
    {
        var (quoteCode, reason) = ParseCodeAndReason(payload);
        if (string.IsNullOrWhiteSpace(quoteCode))
            return "Vui lòng cung cấp mã báo giá. Ví dụ: _Từ chối báo giá QT-2024-001 vì sai đơn giá_";

        if (string.IsNullOrWhiteSpace(reason))
            return "Vui lòng ghi rõ lý do từ chối. Ví dụ: _Từ chối báo giá QT-2024-001 vì sai đơn giá_";

        var search = await mediator.Send(new GetQuoteByCodeQuery(quoteCode), ct);
        if (!search.IsSuccess || search.Value is null)
            return $"Không tìm thấy báo giá với mã *{quoteCode}*";

        var result = await mediator.Send(new RejectQuoteCommand(search.Value.Id, reason), ct);
        if (!result.IsSuccess)
            return $"Không thể từ chối báo giá *{quoteCode}*: {result.Message}";

        return $"Đã từ chối báo giá *{quoteCode}*\n" +
               $"   {search.Value.CompanyName}\n" +
               $"   Lý do: _{EscapeMarkdown(reason)}_";
    }

    private static (string QuoteCode, string Reason) ParseCodeAndReason(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload)) return (string.Empty, string.Empty);

        var separators = new[] { " VI ", " LYDO ", " LY DO ", "|" };
        foreach (var separator in separators)
        {
            var index = payload.IndexOf(separator, StringComparison.OrdinalIgnoreCase);
            if (index < 0) continue;

            var quoteCode = payload[..index].Trim(' ', ':', '-');
            var reason = payload[(index + separator.Length)..].Trim(' ', ':', '-');
            return (quoteCode, reason);
        }

        return (payload.Trim(), string.Empty);
    }
}
