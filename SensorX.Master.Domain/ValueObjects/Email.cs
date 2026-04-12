using System.Text.RegularExpressions;
using SensorX.Master.Domain.Common.Exceptions;

namespace SensorX.Master.Domain.ValueObjects;

public partial record Email
{
    public string Value { get; init; }

    private Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email không được để trống.");

        if (!EmailRegex().IsMatch(value))
            throw new DomainException("Định dạng Email không hợp lệ.");

        Value = value.ToLowerInvariant(); // Chuẩn hóa về chữ thường
    }

    // Factory Method
    public static Email From(string value) => new(value);

    // Chuyển đổi ngầm định sang string để tiện sử dụng
    public static implicit operator string(Email email) => email?.Value ?? string.Empty;

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}