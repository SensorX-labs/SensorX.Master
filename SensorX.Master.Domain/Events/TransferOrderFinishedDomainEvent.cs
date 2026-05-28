using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.Events;

public record TransferOrderFinishedDomainEvent(
    Guid TransferOrderId,
    Guid PickingNoteId,
    Guid ToWarehouseId
) : IDomainEvent;
