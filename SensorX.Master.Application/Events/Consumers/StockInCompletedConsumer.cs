using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Events.IntegrationEvents;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.TransferOrderAggregate;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.TransferOrderAggregate.Specifications;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.SupplyRequestAggregate;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.SupplyRequestAggregate.Specifications;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.ValueObjects;
using SensorX.Master.Application.Common.Interfaces;

namespace SensorX.Master.Application.Events.Consumers;

public class StockInCompletedConsumer(
    IRepository<TransferOrder> transferOrderRepository,
    IRepository<SupplyRequest> supplyRequestRepository,
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
            try
            {
                var sourceCode = Code.From(message.TransferOrderCode);

                // 1. Supply Request logic
                if (message.TransferOrderCode.StartsWith("SR-"))
                {
                    var srSpec = new GetSupplyRequestByCode(sourceCode);
                    var supplyRequest = await supplyRequestRepository.FirstOrDefaultAsync(srSpec, context.CancellationToken);

                    if (supplyRequest is not null && supplyRequest.Status != SupplyRequestStatus.Completed)
                    {
                        supplyRequest.Complete();
                        await supplyRequestRepository.Update(supplyRequest, context.CancellationToken);
                        await unitOfWork.SaveChangesAsync(context.CancellationToken);
                        logger.LogInformation("Updated SupplyRequest {TransferOrderCode} status to Completed due to StockIn", message.TransferOrderCode);
                    }
                    else if (supplyRequest is null)
                    {
                        logger.LogWarning("SupplyRequest {TransferOrderCode} not found for StockIn {StockInId}", message.TransferOrderCode, message.StockInId);
                    }
                    return;
                }

                // 2. Transfer Order logic
                var spec = new GetTransferOrderByCode(sourceCode);
                var transferOrder = await transferOrderRepository.FirstOrDefaultAsync(spec, context.CancellationToken);
                if (transferOrder is not null && transferOrder.Status != TransferOrderStatus.Completed)
                {
                    transferOrder.Complete();
                    await transferOrderRepository.Update(transferOrder, context.CancellationToken);
                    await unitOfWork.SaveChangesAsync(context.CancellationToken);
                    logger.LogInformation("Updated TransferOrder {TransferOrderCode} status to Completed due to StockIn", message.TransferOrderCode);
                }
                else if (transferOrder is null)
                {
                    logger.LogWarning("TransferOrder with Code {TransferOrderCode} not found for StockIn {StockInId}", message.TransferOrderCode, message.StockInId);
                }
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Error processing StockInCreatedEvent for code: {TransferOrderCode}", message.TransferOrderCode);
                throw;
            }
        }
    }
}
