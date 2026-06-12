using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace SensorX.Master.Infrastructure.Services.Telegram;

file record ChatMsg(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content
);

file record ChatReq(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("messages")] List<ChatMsg> Messages,
    [property: JsonPropertyName("temperature")] float Temperature,
    [property: JsonPropertyName("max_tokens")] int MaxTokens,
    [property: JsonPropertyName("stream")] bool Stream = false
);

file record ChatChoice(
    [property: JsonPropertyName("message")] ChatMsg Message
);

file record ChatResp(
    [property: JsonPropertyName("choices")] List<ChatChoice> Choices
);

public partial class TelegramBotBackgroundService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };
    private static readonly Regex OrderCodeRegex = new(@"ORD-[A-Z0-9-]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex QuoteCodeRegex = new(@"QT-[A-Z0-9-]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly string SystemPrompt = """
        Bạn là bộ phân loại ý định cho Telegram bot nội bộ SensorX.

        Nhiệm vụ:
        - Đọc tin nhắn người dùng.
        - Chỉ trả về đúng 1 skill hợp lệ mà backend đang hỗ trợ.
        - Không giải thích.
        - Không trả về JSON.
        - Không trả về tên skill gần đúng.
        - Nếu không khớp thì trả về UNKNOWN.

        Các skill hợp lệ duy nhất:
        - HELP
        - QUOTE_PENDING
        - QUOTE_APPROVED
        - QUOTE_DETAIL:<QUOTE_CODE>
        - QUOTE_APPROVE:<QUOTE_CODE>
        - QUOTE_REJECT:<QUOTE_CODE>|<REASON>
        - ORDER_LIST
        - ORDER_DETAIL:<ORDER_CODE>
        - UNKNOWN

        Quy tắc bắt buộc:
        - Không bao giờ trả về ORDER.
        - Không bao giờ trả về ORDERS.
        - Không bao giờ trả về QUOTE.
        - Không bao giờ trả về QUOTES.
        - Nếu người dùng muốn xem danh sách đơn hàng thì phải trả về ORDER_LIST.
        - Nếu người dùng muốn xem chi tiết đơn hàng và có mã ORD-... thì phải trả về ORDER_DETAIL:<mã>.
        - Nếu người dùng muốn xem báo giá chờ duyệt thì trả về QUOTE_PENDING.
        - Nếu người dùng muốn xem báo giá đã duyệt thì trả về QUOTE_APPROVED.
        - Nếu người dùng muốn xem chi tiết báo giá và có mã QT-... thì trả về QUOTE_DETAIL:<mã>.
        - Nếu người dùng muốn duyệt báo giá và có mã QT-... thì trả về QUOTE_APPROVE:<mã>.
        - Nếu người dùng muốn từ chối báo giá và có mã QT-... cùng lý do thì trả về QUOTE_REJECT:<mã>|<lý do>.
        - Giữ nguyên mã đơn hàng/báo giá từ tin nhắn người dùng.
        - Không thêm khoảng trắng thừa ở đầu hoặc cuối.

        Ví dụ:
        User: /help
        Skill: HELP

        User: menu
        Skill: HELP

        User: danh sách đơn hàng
        Skill: ORDER_LIST

        User: đơn hàng mới
        Skill: ORDER_LIST

        User: chi tiết đơn hàng ORD-2024-001
        Skill: ORDER_DETAIL:ORD-2024-001

        User: báo giá chờ duyệt
        Skill: QUOTE_PENDING

        User: báo giá đã duyệt
        Skill: QUOTE_APPROVED

        User: chi tiết báo giá QT-2024-001
        Skill: QUOTE_DETAIL:QT-2024-001

        User: duyệt báo giá QT-2024-001
        Skill: QUOTE_APPROVE:QT-2024-001

        User: từ chối báo giá QT-2024-001 vì sai đơn giá
        Skill: QUOTE_REJECT:QT-2024-001|sai đơn giá
        """;

    private async Task<string?> ResolveSkillAsync(string userMessage, CancellationToken ct)
    {
        var deterministicSkill = ResolveSkillDeterministically(userMessage);
        if (!string.IsNullOrWhiteSpace(deterministicSkill))
        {
            _logger.LogInformation("[TelegramBot] '{Message}' -> Skill: '{Skill}' (rule)", userMessage, deterministicSkill);
            return deterministicSkill;
        }

        try
        {
            var req = new ChatReq(
                _options.NineRouterModel,
                [new ChatMsg("system", SystemPrompt), new ChatMsg("user", userMessage)],
                0.0f,
                64,
                false
            );

            var client = _httpClientFactory.CreateClient("NineRouter");
            var endpoint = $"{_options.NineRouterUrl.TrimEnd('/')}/v1/chat/completions";

            var resp = await client.PostAsJsonAsync(endpoint, req, ct);
            resp.EnsureSuccessStatusCode();

            var body = await resp.Content.ReadFromJsonAsync<ChatResp>(JsonOpts, ct);
            var rawSkill = body?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();
            var skill = NormalizeSkillName(rawSkill);

            _logger.LogInformation("[TelegramBot] '{Message}' -> Skill: '{Skill}'", userMessage, skill);
            return string.IsNullOrWhiteSpace(skill) || skill == "UNKNOWN" ? null : skill;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TelegramBot] Loi khi goi 9router");
            return null;
        }
    }

    private static string? NormalizeSkillName(string? skill)
    {
        if (string.IsNullOrWhiteSpace(skill))
            return null;

        var normalized = skill.Trim().ToUpperInvariant();

        return normalized switch
        {
            "ORDER" => "ORDER_LIST",
            "ORDERS" => "ORDER_LIST",
            "ORDERS_LIST" => "ORDER_LIST",
            "QUOTE" => "QUOTE_PENDING",
            "QUOTES" => "QUOTE_PENDING",
            _ => normalized
        };
    }

    private static string? ResolveSkillDeterministically(string userMessage)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
            return null;

        var text = userMessage.Trim();
        var normalized = text.ToLowerInvariant();

        if (normalized is "/help" or "/menu" or "help" or "menu")
            return "HELP";

        if (normalized.Contains("chi tiet don hang") || normalized.Contains("chi tiết đơn hàng"))
        {
            var orderCode = OrderCodeRegex.Match(text);
            if (orderCode.Success)
                return $"ORDER_DETAIL:{orderCode.Value.ToUpperInvariant()}";
        }

        if (normalized.Contains("danh sach don hang")
            || normalized.Contains("danh sách đơn hàng")
            || normalized.Contains("don hang moi")
            || normalized.Contains("đơn hàng mới")
            || normalized == "don hang"
            || normalized == "đơn hàng")
        {
            return "ORDER_LIST";
        }

        if (normalized.Contains("chi tiet bao gia") || normalized.Contains("chi tiết báo giá"))
        {
            var quoteCode = QuoteCodeRegex.Match(text);
            if (quoteCode.Success)
                return $"QUOTE_DETAIL:{quoteCode.Value.ToUpperInvariant()}";
        }

        if ((normalized.Contains("duyet bao gia") || normalized.Contains("duyệt báo giá"))
            && QuoteCodeRegex.IsMatch(text))
        {
            var quoteCode = QuoteCodeRegex.Match(text);
            return $"QUOTE_APPROVE:{quoteCode.Value.ToUpperInvariant()}";
        }

        if ((normalized.Contains("tu choi bao gia") || normalized.Contains("từ chối báo giá"))
            && QuoteCodeRegex.IsMatch(text))
        {
            var quoteCode = QuoteCodeRegex.Match(text);
            var reason = ExtractRejectReason(text);
            return string.IsNullOrWhiteSpace(reason)
                ? $"QUOTE_REJECT:{quoteCode.Value.ToUpperInvariant()}|"
                : $"QUOTE_REJECT:{quoteCode.Value.ToUpperInvariant()}|{reason}";
        }

        if (normalized.Contains("bao gia cho duyet")
            || normalized.Contains("báo giá chờ duyệt")
            || normalized.Contains("quote pending"))
        {
            return "QUOTE_PENDING";
        }

        if (normalized.Contains("bao gia da duyet")
            || normalized.Contains("báo giá đã duyệt")
            || normalized.Contains("approved quote")
            || normalized.Contains("approved quotes"))
        {
            return "QUOTE_APPROVED";
        }

        return null;
    }

    private static string ExtractRejectReason(string text)
    {
        var separators = new[] { " vì ", " vi ", "|" };
        foreach (var separator in separators)
        {
            var index = text.IndexOf(separator, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
                return text[(index + separator.Length)..].Trim();
        }

        return string.Empty;
    }
}
