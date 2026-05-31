using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Events.IntegrationEvents;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.TransferOrderAggregate;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.TransferOrderAggregate.Specs;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Application.Common.Interfaces;

namespace SensorX.Master.Application.Events.Consumers;

public class StockInCompletedConsumer(
    IRepository<TransferOrder> transferOrderRepository,
    IUnitOfWork unitOfWork,
    ILogger<StockInCompletedConsumer> logger
) : IConsumer<IStockInCreatedEvent>
{
    public async Task Consume(ConsumeContext<IStockInCreatedEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Master received StockInCreatedEvent: StockInId={StockInId}, TransferOrderCode={TransferOrderCode}", 
            message.StockInId, message.TransferOrderCode);

        if (!string.IsNullOrWhiteSpace(message.TransferOrderCode))
        {
            var spec = new GetTransferOrderByCodeSpec(message.TransferOrderCode);
            var transferOrder = await transferOrderRepository.FirstOrDefaultAsync(spec, context.CancellationToken);
            if (transferOrder != null)
            {
                transferOrder.Complete();
                await transferOrderRepository.Update(transferOrder, context.CancellationToken);
                await unitOfWork.SaveChangesAsync(context.CancellationToken);
                logger.LogInformation("Updated TransferOrder {TransferOrderCode} status to Completed due to StockIn", message.TransferOrderCode);
            }
            else
            {
                logger.LogWarning("TransferOrder with Code {TransferOrderCode} not found for StockIn {StockInId}", message.TransferOrderCode, message.StockInId);
            }
        }
    }
}
