using System.Linq;
using System.Collections.Generic;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;
using SensorX.Master.Domain.Events;

namespace SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.TransferOrderAggregate;

public class TransferOrder : Entity<TransferOrderId>, IAggregateRoot, ICreationTrackable
{
    public Code Code { get; private set; } = null!;
    public WarehouseId SourceWarehouseId { get; private set; } = null!;
    public WarehouseId DestinationWarehouseId { get; private set; } = null!;
    public TransferOrderStatus Status { get; private set; }
    public string Note { get; private set; } = null!;
    public SupplyRequestId? SupplyRequestId { get; private set; }

    private readonly List<TransferOrderItem> _items = new();
    public IReadOnlyList<TransferOrderItem> Items => _items.AsReadOnly();

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;  

    private TransferOrder() : base() { }

    public TransferOrder(TransferOrderId id, Code code, WarehouseId sourceWarehouseId, WarehouseId destinationWarehouseId, TransferOrderStatus status, string note, SupplyRequestId? supplyRequestId = null) : base(id)
    {
        Code = code;
        SourceWarehouseId = sourceWarehouseId;
        DestinationWarehouseId = destinationWarehouseId;
        Status = status;
        Note = note;
        SupplyRequestId = supplyRequestId;

    }

    public void Complete()
    {
        Status = TransferOrderStatus.Completed;
    }

    public void MarkDelivering()
    {
        if (Status == TransferOrderStatus.Processing)
        {
            Status = TransferOrderStatus.Delivering;
        }
    }

    public void AddItem(ProductId productId, Code productCode, string productName, string unit, Quantity quantity, string manufactureName, string note)
    {
        _items.Add(new TransferOrderItem(TransferOrderItemId.New(), productId, productCode, productName, unit, quantity, manufactureName, note));
    }

    public void RaiseCreatedDomainEvent(Guid? pickingNoteId = null)
    {
        var domainItems = _items.Select(x => new TransferOrderCreatedDomainItem(
            x.ProductId.Value,
            x.ProductCode.Value,
            x.ProductName,
            x.Unit,
            x.Quantity.Value,
            x.ManufactureName,
            x.Note
        )).ToList();

        AddDomainEvent(new TransferOrderCreatedDomainEvent(
            Id.Value,
            Code.Value,
            SourceWarehouseId.Value,
            DestinationWarehouseId.Value,
            pickingNoteId ?? Guid.Empty,
            domainItems
        ));
    }
}
