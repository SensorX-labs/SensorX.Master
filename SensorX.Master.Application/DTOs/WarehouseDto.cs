namespace SensorX.Master.Application.DTOs;

public record WarehouseDto(
    Guid Id,
    string Name,
    string? Address,
    string ApiEndpointUrl,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
)
{
    public WarehouseDto() : this(Guid.Empty, string.Empty, null, string.Empty, false, default, null) {}
}

public record CreateWarehouseDto(
    string Name,
    string? Address,
    string ApiEndpointUrl
);