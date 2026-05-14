using System;
using SensorX.Master.Domain.Common.Exceptions;

namespace SensorX.Master.Domain.ValueObjects;

public partial record ApiEndpointUrl
{
    public string Value { get; init; }

    private ApiEndpointUrl(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("API Endpoint URL không được để trống.");

        if (!Uri.TryCreate(value, UriKind.Absolute, out _))
            throw new DomainException("Định dạng URL không hợp lệ.");

        Value = value;
    }

    public static ApiEndpointUrl From(string value) => new(value);

    public static implicit operator string(ApiEndpointUrl url) => url?.Value ?? string.Empty;

    public override string ToString() => Value;
}
