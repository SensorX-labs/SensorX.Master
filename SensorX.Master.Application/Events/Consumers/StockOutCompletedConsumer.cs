using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Events.IntegrationEvents;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.TransferOrderAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Application.Common.Interfaces;

namespace SensorX.Master.Application.Events.Consumers;

public class StockOutCompletedConsumer(
    IRepository<TransferOrder> transferOrderRepository,
    IUnitOfWork unitOfWork,
    ILogger<StockOutCompletedConsumer> logger
) : IConsumer<IStockOutCreatedEvent>
{
    public async Task Consume(ConsumeContext<IStockOutCreatedEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Master received StockOutCreatedEvent: StockOutId={StockOutId}, SourceType={SourceType}, SourceId={SourceId}",
            message.StockOutId, message.SourceType, message.SourceId);

        // SourceType 1 means TransferOrder
        if (message.SourceType == 1)
        {
            var transferOrderId = new TransferOrderId(message.SourceId);
            var transferOrder = await transferOrderRepository.GetByIdAsync(transferOrderId, context.CancellationToken);
            if (transferOrder is not null)
            {
                transferOrder.MarkDelivering();
                await transferOrderRepository.Update(transferOrder, context.CancellationToken);
                await unitOfWork.SaveChangesAsync(context.CancellationToken);
                logger.LogInformation("Updated TransferOrder {TransferOrderId} status to Delivering due to StockOut", transferOrderId.Value);
            }
            else
            {
                logger.LogWarning("TransferOrder {TransferOrderId} not found for StockOut {StockOutId}", transferOrderId.Value, message.StockOutId);
            }
        }
    }
}
