using System.Text.RegularExpressions;
using SensorX.Master.Domain.Common.Exceptions;

namespace SensorX.Master.Domain.ValueObjects;

public partial record Phone
{
    // Sử dụng Regex để validate định dạng số điện thoại Việt Nam (ví dụ)
    private static readonly Regex PhoneRegex = GeneratedPhoneRegex();

    public string Value { get; init; }

    private Phone(string value)
    {
        Value = value;
    }

    public static Phone From(string vlaue) => new(vlaue);

    public static Phone Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Số điện thoại không được để trống.");

        if (!PhoneRegex.IsMatch(value))
            throw new DomainException("Định dạng số điện thoại không hợp lệ.");

        return new Phone(value);
    }

    // C# 11+ Source Generator cho hiệu năng cao hơn
    [GeneratedRegex(@"^(\+84|0)\d{9,10}$")]
    private static partial Regex GeneratedPhoneRegex();

    // Implicit conversion để dễ dàng gán string cho Phone (tùy chọn)
    public static implicit operator string(Phone phone) => phone.Value;
}