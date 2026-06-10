using SensorX.Master.Application.Services;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.PaymentAggregate;
using SensorX.Master.Domain.Contexts.PaymentContext.AggregateModels;

namespace SensorX.Master.Infrastructure.Services;

public class SepayQRBuilder : ISepayQRBuilder
{
    private const string BaseUrl = "https://qr.sepay.vn/img";
    private const string AccountNumber = "0374295407";
    private const string Bank = "MB";
    private const string Template = "null";
    private const string Download = "false";

    public List<string> BuildQRUrls(Payment payment, Order order)
    {
        var orderCode = order.Code.Value;
        return [BuildUrl(payment.Amount.Amount, orderCode)];
    }

    private static string BuildUrl(decimal amount, string des)
    {
        var normalizedAmount = Convert.ToInt64(Math.Round(amount, 0, MidpointRounding.AwayFromZero));
        var encodedDes = Uri.EscapeDataString(des);
        return $"{BaseUrl}?acc={AccountNumber}&bank={Bank}&amount={normalizedAmount}&des={encodedDes}&template={Template}&download={Download}";
    }
}