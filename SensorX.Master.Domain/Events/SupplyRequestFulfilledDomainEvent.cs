using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.Events;

public record SupplyRequestFulfilledDomainEvent(
    Guid SupplyRequestId,
    Guid PickingNoteId,
    Guid WarehouseId
) : IDomainEvent;
