using System;
using SensorX.Master.Domain.Common;

namespace SensorX.Master.Domain.ValueObjects;

public record WarehouseId(Guid Value) : StrongId<WarehouseId>(Value)
{
    public static WarehouseId New() => new(Guid.NewGuid());
}
