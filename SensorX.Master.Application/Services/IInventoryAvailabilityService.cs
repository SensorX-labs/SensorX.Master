using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.Contexts.PaymentContext.AggregateModels;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;

namespace SensorX.Master.Application.Services;

public interface IInventoryAvailabilityService
{
    Task<PaymentType> DeterminePaymentTypeAsync(IReadOnlyCollection<OrderItem> items, CancellationToken cancellationToken = default);
    Task<bool> IsStockSufficientAsync(IReadOnlyCollection<QuoteItem> items, CancellationToken cancellationToken = default);
}