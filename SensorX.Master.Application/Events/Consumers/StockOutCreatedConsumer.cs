using MassTransit;
using SensorX.Master.Application.Events.IntegrationEvents;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using Microsoft.Extensions.Logging;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Application.Events.Consumers;

public class StockOutCreatedConsumer(
    IRepository<Order> orderRepository,
    IUnitOfWork unitOfWork,
    ILogger<StockOutCreatedConsumer> logger
) : IConsumer<StockOutCreatedEvent>
{
    public async Task Consume(ConsumeContext<StockOutCreatedEvent> context)
    {
        var message = context.Message;
        
        // 0 = SalesOrder (DocumentType.SalesOrder)
        if (message.SourceType != 0)
        {
            logger.LogInformation("StockOut {StockOutId} is not for a SalesOrder (SourceType={SourceType}). Skipping.", message.StockOutId, message.SourceType);
            return;
        }

        logger.LogInformation("Master received StockOutCreatedEvent for SalesOrder: {OrderId}", message.SourceId);

        var order = await orderRepository.GetByIdAsync(new OrderId(message.SourceId), context.CancellationToken);

        if (order != null)
        {
            // Update order status to Dispatched
            // wait, Order doesn't have an explicit method for Dispatch. I'll need to check if there is one.
            // Let's use reflection or add a method to Order
            // But since Order entity doesn't have SetDispatched(), we'll check it.
            // Wait, Order Status has a private setter. We need to define a method `Dispatch()` in Order.
            order.Dispatch();
            await orderRepository.Update(order, context.CancellationToken);
            await unitOfWork.SaveChangesAsync(context.CancellationToken);
            logger.LogInformation("Order {OrderId} has been marked as Dispatched.", message.SourceId);
        }
        else
        {
            logger.LogWarning("Order {OrderId} not found for StockOut {StockOutId}", message.SourceId, message.StockOutId);
        }
    }
}
