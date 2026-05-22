using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.Contexts.PaymentContext.AggregateModels;

namespace SensorX.Master.Application.Services;

public interface IInventoryAvailabilityService
{
    Task<PaymentType> DeterminePaymentTypeAsync(IReadOnlyCollection<OrderItem> items, CancellationToken cancellationToken = default);
}