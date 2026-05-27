namespace SensorX.Master.Application.DTOs;

public record WarehouseDto(
    Guid Id,
    string Name,
    string? Address,
    bool IsActive,
    double? Latitude,
    double? Longitude,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    double? Latitude = null,
    double? Longitude = null
)
{
    public WarehouseDto() : this(Guid.Empty, string.Empty, null,  false, null, null, default, null) {}
}

public record CreateWarehouseDto(
    Guid Id,
    string Name,
    string? Address
);