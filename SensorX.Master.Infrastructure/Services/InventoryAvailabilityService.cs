using Microsoft.EntityFrameworkCore;
using SensorX.Master.Application.Services;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.Contexts.PaymentContext.AggregateModels;
using SensorX.Master.Infrastructure.Persistences;

namespace SensorX.Master.Infrastructure.Services;

public class InventoryAvailabilityService(AppDbContext dbContext) : IInventoryAvailabilityService
{
    public async Task<PaymentType> DeterminePaymentTypeAsync(IReadOnlyCollection<OrderItem> items, CancellationToken cancellationToken = default)
    {
        if (items.Count == 0)
        {
            return PaymentType.Partial;
        }

        var productIds = items.Select(item => item.ProductId.Value).Distinct().ToList();

        var inventoryRows = await dbContext.WarehouseInventoryProjections
            .AsNoTracking()
            .Where(row => productIds.Contains(row.ProductId))
            .ToListAsync(cancellationToken);

        var availableByProduct = inventoryRows
            .GroupBy(row => row.ProductId)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(row => row.PhysicalQuantity - row.AllocatedQuantity));

        foreach (var item in items)
        {
            availableByProduct.TryGetValue(item.ProductId.Value, out var available);
            if (available < item.Quantity.Value)
            {
                return PaymentType.Partial;
            }
        }

        return PaymentType.All;
    }
}