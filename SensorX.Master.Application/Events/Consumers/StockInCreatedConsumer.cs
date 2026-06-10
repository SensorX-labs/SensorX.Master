using MassTransit;
using SensorX.Master.Application.Events.IntegrationEvents;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.TransferOrderAggregate;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.SupplyRequestAggregate;
using Microsoft.Extensions.Logging;
using SensorX.Master.Domain.ValueObjects;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.TransferOrderAggregate.Specifications;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.SupplyRequestAggregate.Specifications;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Events.Consumers;

public class StockInCreatedConsumer(
    IRepository<TransferOrder> transferOrderRepository,
    IRepository<SupplyRequest> supplyRequestRepository,
    IUnitOfWork unitOfWork,
    ILogger<StockInCreatedConsumer> logger
) : IConsumer<StockInCreatedEvent>
{
    public async Task Consume(ConsumeContext<StockInCreatedEvent> context)
    {
        var message = context.Message;

        if (string.IsNullOrEmpty(message.TransferOrderCode))
        {
            logger.LogInformation("StockIn {StockInId} does not have a source code. Skipping.", message.StockInId);
            return;
        }

        var sourceCode = Code.From(message.TransferOrderCode);

        // Supply Request logic
        if (message.TransferOrderCode.StartsWith("SR-"))
        {
            logger.LogInformation("Master received StockInCreatedEvent for SupplyRequestCode: {SupplyRequestCode}", message.TransferOrderCode);
            var srSpec = new GetSupplyRequestByCode(sourceCode);
            var supplyRequest = await supplyRequestRepository.FirstOrDefaultAsync(srSpec, context.CancellationToken);

            if (supplyRequest is not null && supplyRequest.Status != SupplyRequestStatus.Completed)
            {
                supplyRequest.Complete();
                await supplyRequestRepository.Update(supplyRequest, context.CancellationToken);
                await unitOfWork.SaveChangesAsync(context.CancellationToken);
                logger.LogInformation("SupplyRequest {SupplyRequestCode} has been marked as Completed.", message.TransferOrderCode);
            }
            else if (supplyRequest is null)
            {
                logger.LogWarning("SupplyRequest {SupplyRequestCode} not found for StockIn {StockInId}", message.TransferOrderCode, message.StockInId);
            }
            return;
        }

        // Transfer Order logic
        logger.LogInformation("Master received StockInCreatedEvent for TransferOrderCode: {TransferOrderCode}", message.TransferOrderCode);

        var spec = new GetTransferOrderByCode(sourceCode);
        var transferOrder = await transferOrderRepository.FirstOrDefaultAsync(spec, context.CancellationToken);

        if (transferOrder is not null && transferOrder.Status != TransferOrderStatus.Completed)
        {
            transferOrder.Complete();
            await transferOrderRepository.Update(transferOrder, context.CancellationToken);
            await unitOfWork.SaveChangesAsync(context.CancellationToken);
            logger.LogInformation("TransferOrder {TransferOrderCode} has been marked as Completed.", message.TransferOrderCode);
        }
        else if (transferOrder is null)
        {
            logger.LogWarning("TransferOrder {TransferOrderCode} not found for StockIn {StockInId}", message.TransferOrderCode, message.StockInId);
        }
    }
}
