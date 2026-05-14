using System;
using SensorX.Master.Domain.Common;

namespace SensorX.Master.Domain.ValueObjects;

public record ApiEndpointUrl(string Value)
{
    public static ApiEndpointUrl From(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            throw new ArgumentException("Invalid URL format.");
        return new ApiEndpointUrl(url);
    }
}