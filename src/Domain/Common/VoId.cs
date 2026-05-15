using System;
using System.Collections.Generic;

namespace SensorX.Master.Domain.Common;

public abstract class VoId : IEquatable<VoId>
{
    protected VoId() { }
    public abstract string Value { get; }
    public override bool Equals(object? obj) => Equals(obj as VoId);
    public bool Equals(VoId? other) => !(other is null) && Value == other.Value;
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(VoId? left, VoId? right) => Equals(left, right);
    public static bool operator !=(VoId? left, VoId? right) => !(left == right);
}
