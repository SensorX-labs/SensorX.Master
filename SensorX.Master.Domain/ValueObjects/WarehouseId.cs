
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.ValueObjects;

public record WarehouseId(Guid Value) : EntityId<WarehouseId>(Value)
{
    public static readonly WarehouseId Default = new(Guid.Parse("018e7b5a-1a5c-7b9e-8d4f-c3b2a1a0b9c8"));
}