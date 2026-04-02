using System.Text.RegularExpressions;
using SensorX.Master.Domain.Common.Exceptions;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.ValueObjects;

public record Code
{
    private static readonly Regex CodeRegex = new(@"^[A-Z]+-\d{6}-\d{9}$", RegexOptions.Compiled);

    public string Value { get; init; }

    private Code(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Code cannot be empty.");
        //kiểm tra định dạng $"{prefix.ToUpper()}-{now:yyMMdd}-{now:HHmmssfff}";
        if (!CodeRegex.IsMatch(value))
            throw new DomainException("Code format is invalid.");
        Value = value;
    }

    public static Code From(string value) => new(value);

    public static Code Create(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            throw new DomainException("Prefix cannot be empty.");

        var now = DateTime.UtcNow;
        var code = $"{prefix.ToUpper()}-{now:yyMMdd}-{now:HHmmssfff}";
        return new Code(code);
    }

    public static implicit operator string(Code code) => code?.Value ?? string.Empty;

    public override string ToString() => Value;
}