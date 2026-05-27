
using System.Collections.Generic;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.Events;

public record TransferOrderCreatedDomainItem(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    string Unit,
    int Quantity,
    string ManufactureName,
    string Note
);

public record TransferOrderCreatedDomainEvent(
    Guid TransferOrderId,
    string TransferOrderCode,
    Guid FromWarehouseId,
    Guid ToWarehouseId,
    Guid PickingNoteId,
    List<TransferOrderCreatedDomainItem> Items
) : IDomainEvent;
