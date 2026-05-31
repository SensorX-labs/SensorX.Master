using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace SensorX.Master.Infrastructure.Services.Telegram;

// ─── 9router OpenAI-compatible DTOs ───────────────────────────────────────────

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

    // Prompt mặc định đã tối ưu - thêm skill mới trực tiếp vào đây
    private static readonly string SystemPrompt = """
        Bạn là bộ phân loại ý định cho chatbot doanh nghiệp SensorX.
        Phân tích tin nhắn và trả về tên skill phù hợp nhất.

        Danh sách skills:
        - HELP: Khi người dùng hỏi help, menu, hướng dẫn, trợ giúp, bot làm được gì
        - QUOTE_PENDING: Báo giá đang chờ duyệt, báo giá chờ phê duyệt, quote pending
        - QUOTE_APPROVED: Danh sách báo giá đã duyệt, báo giá được chấp nhận, approved quotes
        - QUOTE_DETAIL: Xem chi tiết một báo giá cụ thể. Trả về: QUOTE_DETAIL:<mã báo giá>
        - QUOTE_APPROVE: Duyệt/phê duyệt một báo giá cụ thể. Trả về: QUOTE_APPROVE:<mã báo giá> (VD: QUOTE_APPROVE:QT-2024-001)
        - QUOTE_REJECT: Từ chối một báo giá cụ thể. Trả về: QUOTE_REJECT:<mã báo giá>|<lý do>
        - UNKNOWN: Không có skill nào phù hợp.

        Quy tắc: Chỉ trả về DUY NHẤT tên skill. KHÔNG giải thích, KHÔNG thêm văn bản khác.
        """;

    private async Task<string?> ResolveSkillAsync(string userMessage, CancellationToken ct)
    {
        try
        {
            var req = new ChatReq(_options.NineRouterModel,
                [new ChatMsg("system", SystemPrompt), new ChatMsg("user", userMessage)],
                0.05f, 50, false);

            var client = _httpClientFactory.CreateClient("NineRouter");
            var endpoint = $"{_options.NineRouterUrl.TrimEnd('/')}/v1/chat/completions";

            var resp = await client.PostAsJsonAsync(endpoint, req, ct);
            resp.EnsureSuccessStatusCode();

            var body = await resp.Content.ReadFromJsonAsync<ChatResp>(JsonOpts, ct);
            var skill = body?.Choices?.FirstOrDefault()?.Message?.Content?.Trim().ToUpper();

            _logger.LogInformation("[TelegramBot] '{Message}' → Skill: '{Skill}'", userMessage, skill);
            return (string.IsNullOrWhiteSpace(skill) || skill == "UNKNOWN") ? null : skill;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TelegramBot] Lỗi khi gọi 9router");
            return null;
        }
    }
}
