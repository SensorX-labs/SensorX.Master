
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.Events;

public record TransferOrderCreatedDomainEvent(
    Guid TransferOrderId,
    string TransferOrderCode,
    Guid FromWarehouseId
) : IDomainEvent;
