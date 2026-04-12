using SensorX.Master.Domain.Common.Exceptions;

namespace SensorX.Master.Domain.ValueObjects;

public record Percent
{
    public decimal Value { get; }

    private Percent(decimal value)
    {
        // Thường Percent trong kinh doanh nằm từ 0 - 100
        // Tuy nhiên có trường hợp tăng trưởng > 100% nên chỉ chặn số âm
        if (value < 0)
        {
            throw new DomainException("Tỉ lệ phần trăm không được âm.");
        }
        Value = value;
    }

    // Factory Methods
    public static Percent From(decimal value) => new(value);
    public static Percent Zero => new(0);

    // Helper tính toán: ví dụ 10% của 1,000,000
    public decimal CalculateAmount(decimal totalAmount)
    {
        return totalAmount * (Value / 100);
    }

    // Implicit operators để dễ dàng tính toán với decimal
    public static implicit operator decimal(Percent percent) => percent.Value;

    public static Percent operator +(Percent a, Percent b) => new(a.Value + b.Value);
    public static Percent operator -(Percent a, Percent b) => new(a.Value - b.Value);

    public override string ToString() => $"{Value}%";
}