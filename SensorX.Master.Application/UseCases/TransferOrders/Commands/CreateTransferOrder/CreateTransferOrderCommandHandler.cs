using MediatR;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.TransferOrderAggregate;
using SensorX.Master.Domain.Events;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.UseCases.TransferOrders.Commands.CreateTransferOrder;

public class CreateTransferOrderCommandHandler(
    IRepository<TransferOrder> transferOrderRepository,
    IMediator mediator
) : IRequestHandler<CreateTransferOrderCommand, Guid>
{
    public async Task<Guid> Handle(CreateTransferOrderCommand request, CancellationToken cancellationToken)
    {
        var code = Code.From(request.Code);
        var sourceWarehouseId = new WarehouseId(request.SourceWarehouseId);
        var destinationWarehouseId = new WarehouseId(request.DestinationWarehouseId);

        var transferOrder = new TransferOrder(
            new TransferOrderId(Guid.NewGuid()),
            code,
            sourceWarehouseId,
            destinationWarehouseId,
            TransferOrderStatus.Processing, // Correct status
            request.Note,
            null // SupplyRequestId is optional
        );

        await transferOrderRepository.AddAsync(transferOrder, cancellationToken);

        // Publish domain event
        await mediator.Publish(new TransferOrderCreatedDomainEvent(
            transferOrder.Id.Value,
            code.Value
        ), cancellationToken);

        return transferOrder.Id.Value;
    }
}
