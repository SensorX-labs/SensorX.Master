using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
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
}
