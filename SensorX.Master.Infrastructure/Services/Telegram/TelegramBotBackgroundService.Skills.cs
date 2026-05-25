using MediatR;
using SensorX.Master.Application.Commands.Quotes.ApproveQuote;
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

        if (!result.IsSuccess || result.Value is null) return "Khong the truy van bao gia tu database.";

        var items = result.Value.Items.AsEnumerable();
        if (statusFilter == "Pending")
        {
            string[] pending = ["PendingApproval", "Pending", "Submitted"];
            items = items.Where(q => pending.Any(s => q.Status.ToString().Contains(s, StringComparison.OrdinalIgnoreCase)));
            return FormatQuoteList(items.ToList(), "Bao Gia Cho Duyet");
        }
        else if (statusFilter == "Approved")
        {
            items = items.Where(q => q.Status.ToString().Contains("Approved", StringComparison.OrdinalIgnoreCase));
            return FormatQuoteList(items.ToList(), "Bao Gia Da Duyet");
        }

        return FormatQuoteList(items.ToList(), "Danh Sach Bao Gia");
    }

    private async Task<string> HandleQuoteApprove(IMediator mediator, string quoteCode, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(quoteCode))
            return "Vui long cung cap ma bao gia. Vi du: _Duyet bao gia QT-2024-001_";

        var search = await mediator.Send(new GetQuoteByCodeQuery(quoteCode), ct);
        if (!search.IsSuccess || search.Value is null)
            return $"Khong tim thay bao gia voi ma *{quoteCode}*";

        var quote = search.Value;

        var result = await mediator.Send(new ApproveQuoteCommand(quote.Id), ct);
        if (!result.IsSuccess)
            return $"Khong the duyet bao gia *{quoteCode}*: {result.Message}";

        return $"Da phe duyet thanh cong bao gia *{quoteCode}*\n" +
               $"   {quote.CompanyName}\n" +
               $"   `{quote.GrandTotal:N0} VND`";
    }
}
