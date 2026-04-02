using SensorX.Master.Domain.Common.Exceptions;

namespace SensorX.Master.Domain.ValueObjects;

public record Quantity
{
    public int Value { get; }

    public Quantity(int value)
    {
        if (value < 0)
        {
            throw new DomainException("Quantity cannot be negative");
        }
        Value = value;
    }

    public static implicit operator int(Quantity quantity) => quantity.Value;
    public static Quantity operator -(Quantity a, int b) => new(a.Value - b);
    public static Quantity operator +(Quantity a, int b) => new(a.Value + b);
    public static Quantity operator -(Quantity a, Quantity b) => new(a.Value - b.Value);
    public static Quantity operator +(Quantity a, Quantity b) => new(a.Value + b.Value);
}