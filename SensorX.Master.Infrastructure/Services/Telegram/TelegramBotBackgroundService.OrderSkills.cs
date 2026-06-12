using MediatR;
using SensorX.Master.Application.Queries.Orders.GetDetailOrderById;
using SensorX.Master.Application.Queries.Orders.GetPageListOrder;

namespace SensorX.Master.Infrastructure.Services.Telegram;

public partial class TelegramBotBackgroundService
{
    /// <summary>
    /// Danh sách 10 đơn hàng gần nhất (tất cả trạng thái).
    /// </summary>
    private async Task<string> HandleOrdersAsync(IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(
            new GetPageListOrderQuery { PageNumber = 1, PageSize = 10 },
            ct
        );

        if (!result.IsSuccess || result.Value is null)
            return "Không thể truy vấn đơn hàng từ database.";

        return FormatOrderList(result.Value.Items, "Đơn Hàng Gần Nhất");
    }

    /// <summary>
    /// Chi tiết đơn hàng theo mã code, bao gồm thông tin thanh toán và sản phẩm.
    /// </summary>
    private async Task<string> HandleOrderDetailAsync(IMediator mediator, string orderCode, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(orderCode))
            return "Vui lòng cung cấp mã đơn hàng. Ví dụ: _Chi tiết đơn hàng ORD-2024-001_";

        var search = await mediator.Send(
            new GetPageListOrderQuery
            {
                Code = orderCode,
                PageNumber = 1,
                PageSize = 1
            },
            ct
        );

        var matchedOrder = search.Value?.Items.FirstOrDefault();

        if (!search.IsSuccess || matchedOrder is null)
            return $"Không tìm thấy đơn hàng với mã *{EscapeMarkdown(orderCode)}*";

        var detail = await mediator.Send(new GetDetailOrderByIdQuery(matchedOrder.Id), ct);
        if (!detail.IsSuccess || detail.Value is null)
            return $"Không lấy được chi tiết đơn hàng *{EscapeMarkdown(orderCode)}*";

        return FormatOrderDetail(detail.Value);
    }
}
