using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.PaymentAggregate;

namespace SensorX.Master.Application.Services;

public interface ISepayQRBuilder
{
    List<string> BuildQRUrls(Payment payment, Order order);
}