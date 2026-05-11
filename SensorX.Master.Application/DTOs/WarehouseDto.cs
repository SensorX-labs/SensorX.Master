namespace SensorX.Master.Application.DTOs;

public record WarehouseDto(
    Guid Id,
    string Name,
    string? Address,
    string ApiEndpointUrl,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

public record CreateWarehouseDto(
    string Name,
    string? Address,
    string ApiEndpointUrl
);