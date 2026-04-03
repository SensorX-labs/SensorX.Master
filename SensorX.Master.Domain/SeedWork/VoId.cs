namespace SensorX.Master.Domain.SeedWork;

public record VoId(Guid Value)
{
    public static implicit operator Guid(VoId voId) => voId?.Value ?? Guid.Empty;
    public override string ToString() => Value.ToString();
}