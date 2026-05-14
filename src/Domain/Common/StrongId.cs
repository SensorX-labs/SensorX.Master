using System;

namespace SensorX.Master.Domain.Common;

public abstract record StrongId<TId>(Guid Value)
{
    public override string ToString() => Value.ToString();
}
