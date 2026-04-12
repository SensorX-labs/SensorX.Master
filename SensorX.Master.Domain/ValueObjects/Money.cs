using SensorX.Master.Domain.Common.Exceptions;

namespace SensorX.Master.Domain.ValueObjects;

public record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; }

    // Private constructor để bắt buộc dùng Factory Method hoặc khởi tạo đúng
    private Money(decimal amount, string currency = "VND")
    {
        if (amount < 0) throw new DomainException("Số tiền không được âm.");
        Amount = amount;
        Currency = currency;
    }

    // Factory Method cho VND
    public static Money FromVnd(decimal amount) => new(amount, "VND");
    public static Money Zero(string currency = "VND") => new(0, currency);

    // Các phép toán cơ bản (Overload operators)
    public static Money operator +(Money a, Money b)
    {
        CheckSameCurrency(a, b);
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        CheckSameCurrency(a, b);
        return new Money(a.Amount - b.Amount, a.Currency);
    }

    public static Money operator *(Money m, Quantity q)
    {
        return new Money(m.Amount * q.Value, m.Currency);
    }

    public static Money operator *(Money m, Percent p)
    {
        return new Money(m.Amount * (p.Value / 100), m.Currency);
    }

    public static bool operator <(Money a, Money b)
    {
        CheckSameCurrency(a, b);
        return a.Amount < b.Amount;
    }

    public static bool operator >(Money a, Money b) => b < a;

    private static void CheckSameCurrency(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new DomainException("Không thể tính toán giữa hai loại tiền tệ khác nhau.");
    }

    public override string ToString() => $"{Amount:N0} {Currency}";
}