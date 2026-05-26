namespace SensorX.Master.Application.DTOs;

public record WarehouseDto(
    Guid Id,
    string Name,
    string? Address,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    double? Latitude = null,
    double? Longitude = null
)
{
    public WarehouseDto() : this(Guid.Empty, string.Empty, null,  false, default, null, null, null) {}
}

public record CreateWarehouseDto(
    Guid Id,
    string Name,
    string? Address
);